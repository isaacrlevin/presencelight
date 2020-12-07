using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.SnapshotCollector;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using PresenceLight.Core;
using PresenceLight.Core.Graph;
using PresenceLight.Services;
using PresenceLight.Telemetry;

namespace PresenceLight
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private class SnapshotCollectorTelemetryProcessorFactory : ITelemetryProcessorFactory
        {
            private readonly IServiceProvider _serviceProvider;
            public SnapshotCollectorTelemetryProcessorFactory(IServiceProvider serviceProvider) =>
                _serviceProvider = serviceProvider;
            public ITelemetryProcessor Create(ITelemetryProcessor next)
            {
                var snapshotConfigurationOptions = _serviceProvider.GetService<IOptions<SnapshotCollectorConfiguration>>();
                return new SnapshotCollectorTelemetryProcessor(next, configuration: snapshotConfigurationOptions == null ? new SnapshotCollectorConfiguration() : snapshotConfigurationOptions.Value);
            }
        }

        public IServiceProvider? ServiceProvider { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public App()
        {
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            if (SingleInstanceAppMutex.TakeExclusivity())
            {
                Exit += (_, __) => SingleInstanceAppMutex.ReleaseExclusivity();

                try
                {
                    ContinueStartup();
                }
                catch (Exception ex) when (IsCriticalFontLoadFailure(ex))
                {
                    Trace.WriteLine($"## Warning Notify ##: {ex}");
                    OnCriticalFontLoadFailure();
                }
            }
            else
            {
                Shutdown();
            }
        }

        private void ContinueStartup()
        {
            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();


            IServiceCollection services = new ServiceCollection();
            services.AddOptions();
            services.Configure<ConfigWrapper>(Configuration);
            services.Configure<TelemetryConfiguration>(
    (o) =>
    {
        o.InstrumentationKey = Configuration["ApplicationInsights:InstrumentationKey"];
        o.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
        o.TelemetryInitializers.Add(new AppVersionTelemetryInitializer());
        o.TelemetryInitializers.Add(new EnvironmentTelemetryInitializer());

        if (Convert.ToBoolean(Configuration["SendDiagnosticData"], CultureInfo.InvariantCulture))
        {
            o.TelemetryProcessorChainBuilder.UseSnapshotCollector(new SnapshotCollectorConfiguration
            {
                IsEnabled = true,
                IsEnabledInDeveloperMode = true,
                DeoptimizeMethodCount = 4

            });
        }
    });
            services.AddApplicationInsightsTelemetryWorkerService();

            services.AddSingleton<IGraphService, GraphService>();
            services.AddSingleton<IHueService, HueService>();
            services.AddSingleton<LIFXService, LIFXService>();
            services.AddSingleton<IYeelightService, YeelightService>();
            services.AddSingleton<ICustomApiService, CustomApiService>();
            services.AddSingleton<LIFXOAuthHelper, LIFXOAuthHelper>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<SettingsService, SettingsService>();
            services.AddTransient<DiagnosticsClient, DiagnosticsClient>();

            ServiceProvider = services.BuildServiceProvider();

            var telemetryClient = ServiceProvider.GetRequiredService<TelemetryClient>();


            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private static bool IsCriticalFontLoadFailure(Exception ex)
        {
            return ex.StackTrace.Contains("MS.Internal.Text.TextInterface.FontFamily.GetFirstMatchingFont", StringComparison.OrdinalIgnoreCase) ||
                   ex.StackTrace.Contains("MS.Internal.Text.Line.Format", StringComparison.OrdinalIgnoreCase);
        }

        private static void OnCriticalFontLoadFailure()
        {
            Trace.WriteLine($"App OnCriticalFontLoadFailure");

            new Thread(() =>
            {
                if (MessageBox.Show(
                    "PresenceLight Is Already Running",
                    "PresenceLight Could Note Start",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK) == MessageBoxResult.OK)
                {
                    Trace.WriteLine($"App OnCriticalFontLoadFailure OK");
                }
                Environment.Exit(0);
            }).Start();

            // Stop execution because callbacks to the UI thread will likely cause another cascading font error.
            new AutoResetEvent(false).WaitOne();
        }

    }
}

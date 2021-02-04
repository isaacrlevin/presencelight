using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NLog;
using NLog.Extensions.Logging;

using PresenceLight.Core;
using PresenceLight.Graph;
using PresenceLight.Services;
using PresenceLight.Telemetry;

namespace PresenceLight
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public IServiceProvider? ServiceProvider { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public static IConfiguration StaticConfig { get; private set; }

        public App()
        { }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            if (SingleInstanceAppMutex.TakeExclusivity())
            {
                Exit += (_, __) => SingleInstanceAppMutex.ReleaseExclusivity();
                var logger = LogManager.GetCurrentClassLogger();
                logger.Debug("Starting PresenceLight");
                try
                {
                    ContinueStartup();
                }
                catch (Exception ex) when (IsCriticalFontLoadFailure(ex))
                {
                    Trace.WriteLine($"## Warning Notify ##: {ex}");
                    logger.Error(ex, "Stopped program because of exception");
                    OnCriticalFontLoadFailure();
                }
            }
            else
            {
                LogManager.Shutdown();
                Shutdown();
            }
        }

        private void ContinueStartup()
        {
            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                 .AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true);


            Configuration = builder.Build();
            StaticConfig = builder.Build();

            IServiceCollection services = new ServiceCollection();
            services.AddOptions();
            services.AddLogging(logging =>
            {
                logging.AddNLog();
            });
            services.Configure<BaseConfig>(Configuration);
            services.Configure<AADSettings>(Configuration.GetSection("AADSettings"));
            services.Configure<TelemetryConfiguration>(
    (o) =>
    {
        o.InstrumentationKey = Configuration["ApplicationInsights:InstrumentationKey"];
        o.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
        o.TelemetryInitializers.Add(new AppVersionTelemetryInitializer());
        o.TelemetryInitializers.Add(new EnvironmentTelemetryInitializer());

    });
            services.AddApplicationInsightsTelemetryWorkerService(options =>
        {
            options.EnablePerformanceCounterCollectionModule = false;
            options.EnableDependencyTrackingTelemetryModule = false;
        });

            services.AddSingleton<IGraphService, GraphService>();
            services.AddSingleton<IHueService, HueService>();
            services.AddSingleton<IRemoteHueService, RemoteHueService>();
            services.AddSingleton<LIFXService, LIFXService>();
            services.AddSingleton<IYeelightService, YeelightService>();
            services.AddSingleton<ICustomApiService, CustomApiService>();
            services.AddSingleton<GraphWrapper, GraphWrapper>();
            services.AddSingleton<IWorkingHoursService, WorkingHoursService>();
            services.AddSingleton<LIFXOAuthHelper, LIFXOAuthHelper>();
            services.AddSingleton<ThisAppInfo, ThisAppInfo>();
            services.AddSingleton<MainWindow>();

            if (Convert.ToBoolean(Configuration["IsAppPackaged"], CultureInfo.InvariantCulture))
            {
                services.AddSingleton<ISettingsService, AppPackageSettingsService>();
            }
            else
            {
                services.AddSingleton<ISettingsService, StandaloneSettingsService>();
            }

            services.AddTransient<DiagnosticsClient, DiagnosticsClient>();

            ServiceProvider = services.BuildServiceProvider();

            var configuration = ServiceProvider.GetService<TelemetryConfiguration>();

            if (configuration != null)
            {
                var b = configuration.DefaultTelemetrySink.TelemetryProcessorChainBuilder;
                double fixedSamplingPercentage = 10;
                b.UseSampling(fixedSamplingPercentage);
                b.Build();
            }
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

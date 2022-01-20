using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;

using MediatR;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using PresenceLight.Core;
using PresenceLight.Graph;
using PresenceLight.Services;
using PresenceLight.Telemetry;

using Serilog;

using Windows.Storage;

namespace PresenceLight
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public IServiceProvider? ServiceProvider { get; private set; }

        public IConfiguration? Configuration { get; private set; }

        public static IConfiguration? StaticConfig { get; private set; }

     
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
                    Log.Error(ex, "Stopped program because of exception");
                    OnCriticalFontLoadFailure();
                }
            }
            else
            {
                Log.CloseAndFlush();
                Shutdown();
            }
        }

        Dictionary<string, string> InMemorySettings = new();

        private void ContinueStartup()
        {
            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                 .AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true);


            Configuration = builder.Build();
            StaticConfig = builder.Build();

            //Override the save file location for logs if this is a packaged app... 
            if (Convert.ToBoolean(Configuration["IsAppPackaged"], CultureInfo.InvariantCulture))
            {
                var _logFilePath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "PresenceLight\\logs\\DesktopClient\\log-.json");

                InMemorySettings.Add("Serilog:WriteTo:1:Args:Path", _logFilePath);
                builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                 .AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true)
                 .AddInMemoryCollection(InMemorySettings);

                Configuration = builder.Build();
                StaticConfig = builder.Build();

            }

            var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.InstrumentationKey = Configuration["ApplicationInsights:InstrumentationKey"];
            var loggerConfig =
            new LoggerConfiguration()
                          .ReadFrom.Configuration(Configuration)
                          .WriteTo.PresenceEventsLogSink()
                          .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces, Serilog.Events.LogEventLevel.Error)
                          .Enrich.FromLogContext();


            Log.Logger = loggerConfig.CreateLogger();
            Log.Debug("Starting PresenceLight");

            IServiceCollection services = new ServiceCollection();
            services.AddOptions();
            services.AddLogging(logging =>
            {
                logging.AddSerilog();
            });

            //Need to tell MediatR what Assemblies to look in for Command Event Handlers
            services.AddMediatR(typeof(App),
                                typeof(PresenceLight.Core.BaseConfig));

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

            services.AddPresenceServices();

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

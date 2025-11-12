using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;

using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MudBlazor.Services;

using PresenceLight.Core;
using PresenceLight.Razor;
using PresenceLight.Razor.Services;
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
            IServiceCollection services = new ServiceCollection();

            // Configuration Section
            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                 .AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true);

            string userAppSettings;

            //Override the save file location for logs if this is a packaged app... 
            if (new DesktopBridge.Helpers().IsRunningAsUwp())
            {
                var _logFilePath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "PresenceLight\\logs\\DesktopClient\\log-.json");
                InMemorySettings.Add("Serilog:WriteTo:1:Args:Path", _logFilePath);
                builder.AddInMemoryCollection(InMemorySettings);

                userAppSettings = AppPackageSettingsService.BuildSettingsFileLocation();
            }
            else
            {
                userAppSettings = StandaloneSettingsService.BuildSettingsFileLocation();
            }
            
            builder.AddJsonFile(userAppSettings, optional: true, reloadOnChange: true);

            Configuration = builder.Build();

            services.Configure<BaseConfig>(Configuration);
            services.AddSingleton(Configuration);
            services.AddOptions();
            services.Configure<AADSettings>(Configuration.GetSection("AADSettings"));

            //Logging
            var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.InstrumentationKey = Configuration["ApplicationInsights:InstrumentationKey"];



            var loggerConfig =
            new LoggerConfiguration()
                          .ReadFrom.Configuration(Configuration)
                          .WriteTo.PresenceEventsLogSink()
                          .Enrich.FromLogContext();


            Log.Logger = loggerConfig.CreateLogger();
            Log.Debug("Starting PresenceLight");

            services.AddLogging(logging =>
            {
                logging.AddSerilog();
            });

#if DEBUG
            services.AddBlazorWebViewDeveloperTools();
#endif


            services.Configure<TelemetryConfiguration>((o) =>
            {
                o.InstrumentationKey = Configuration["ApplicationInsights:InstrumentationKey"];
                o.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
            });
            services.AddApplicationInsightsTelemetryWorkerService(options =>
            {
                options.EnablePerformanceCounterCollectionModule = false;
                options.EnableDependencyTrackingTelemetryModule = false;
            });



            //Blazor

            services.AddMudServices();

            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddWpfBlazorWebView();

            services.AddBlazorise(options =>
                 {
                     options.Immediate = true;
                 })
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(App).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(BaseConfig).Assembly);
            });

            //Singleton Services
            services.AddSingleton<AppState>();
            services.AddSingleton<AppInfo, AppInfo>();

            services.AddSingleton<LoginService, LoginService>();
            services.AddSingleton<AuthorizationProvider, AuthorizationProvider>();



            services.AddPresenceServices();

            services.AddSingleton<LIFXOAuthHelper, LIFXOAuthHelper>();
            services.AddSingleton<MainWindow>();
            services.AddTransient<DiagnosticsClient, DiagnosticsClient>();

            if (new DesktopBridge.Helpers().IsRunningAsUwp())
            {
                services.AddSingleton<ISettingsService, AppPackageSettingsService>();
            }
            else
            {
                services.AddSingleton<ISettingsService, StandaloneSettingsService>();
            }

            services.AddSingleton<ITelemetryInitializer, AppVersionTelemetryInitializer>();

            //Inject Services Into MainWindow
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
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using MediatR;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using PresenceLight.Core;
using PresenceLight.Core.CustomApiServices;
using PresenceLight.Core.PubSub;
using PresenceLight.Graph;
using PresenceLight.Services;
using PresenceLight.Telemetry;
using PresenceLight.ViewModels;

using Serilog;

using Windows.Storage;

namespace PresenceLight
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        public static IConfiguration? Configuration { get; private set; }

        public App()
        {
            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables();

            Configuration = builder.Build();
            if (Convert.ToBoolean(Configuration["IsAppPackaged"], CultureInfo.InvariantCulture))
            {
                var _logFilePath = System.IO.Path.Combine(ApplicationData.Current.LocalFolder.Path, "PresenceLight\\logs\\DesktopClient\\log-.json");

                InMemorySettings.Add("Serilog:WriteTo:1:Args:Path", _logFilePath);
                builder.AddInMemoryCollection(InMemorySettings);
                Configuration = builder.Build();
            }

            var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.InstrumentationKey = Configuration?["ApplicationInsights:InstrumentationKey"];

            var loggerConfig = new LoggerConfiguration()
                                      .ReadFrom.Configuration(Configuration)
                                      .WriteTo.PresenceEventsLogSink()
                                      .WriteTo.Console()
                                      .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces, Serilog.Events.LogEventLevel.Error)
                                      .Enrich.FromLogContext();


#if DEBUG
            Serilog.Debugging.SelfLog.Enable(Console.Out);
#endif
            Log.Logger = loggerConfig.CreateLogger();
            Log.Debug("Starting PresenceLight");
        }

        private async Task ContinueStartup()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddOptions();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains("PresenceLight")).ToArray();

            //Need to tell MediatR what Assemblies to look in for Command Event Handlers
            services.AddMediatR(assemblies);

            //Override the save file location for logs if this is a packaged app... 

            services.Configure<BaseConfig>(Configuration);
            services.Configure<AADSettings>(Configuration?.GetSection("AADSettings"));
            services.Configure<TelemetryConfiguration>(
            (o) =>
            {
                o.InstrumentationKey = Configuration?["ApplicationInsights:InstrumentationKey"];
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

            if (Convert.ToBoolean(Configuration?["MockServices"], CultureInfo.InvariantCulture))
            {
                services.AddMockLightServices();
            }
            else
            {
                services.AddPresenceServices();
            }

            services.AddSingleton<LIFXOAuthHelper>();
            services.AddSingleton<ThisAppInfo>();
            services.AddTransient<DiagnosticsClient>();
            services.AddViewModels();
            services.AddSingleton<MainWindowModern>();

            if (Convert.ToBoolean(Configuration?["IsAppPackaged"], CultureInfo.InvariantCulture))
            {
                services.AddSingleton<ISettingsService, AppPackageSettingsService>();
            }
            else
            {
                services.AddSingleton<ISettingsService, StandaloneSettingsService>();
            }

            ServiceProvider = services.BuildServiceProvider();
            SettingsHandlerBase.Options = ServiceProvider.GetRequiredService<IOptionsMonitor<BaseConfig>>().CurrentValue;
            SettingsHandlerBase.Config = ServiceProvider.GetRequiredService<IOptionsMonitor<BaseConfig>>().CurrentValue;
            SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTimeAsDate = string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTime) ? null : DateTime.Parse(SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTime, null);
            SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTimeAsDate = string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTime) ? null : DateTime.Parse(SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTime, null);

            var mediator = ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new LoadSettingsCommand());
            await mediator.Publish(new InitializeNotification(SettingsHandlerBase.Config));

            var mainWindow = ServiceProvider.GetRequiredService<MainWindowModern>();

            if (Convert.ToBoolean(Configuration?["StartMinimized"], CultureInfo.InvariantCulture))
            {
                mainWindow.Hide();
            }
            else
            {
                mainWindow.Show();
            }
        }


        private async void OnStartup(object sender, StartupEventArgs e)
        {
            if (SingleInstanceAppMutex.TakeExclusivity())
            {
                Exit += (_, __) => SingleInstanceAppMutex.ReleaseExclusivity();

                try
                {
                    await ContinueStartup();
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
        private async void OnExit(object sender, ExitEventArgs e)
        {

        }
        Dictionary<string, string> InMemorySettings = new();



        private static bool IsCriticalFontLoadFailure(Exception ex)
        {
            return (ex?.StackTrace?.Contains("MS.Internal.Text.TextInterface.FontFamily.GetFirstMatchingFont", StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (ex?.StackTrace?.Contains("MS.Internal.Text.Line.Format", StringComparison.OrdinalIgnoreCase) ?? false);
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

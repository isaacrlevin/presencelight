using System;
using PresenceLight.Core;
using PresenceLight.Core.Graph;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PresenceLight.Telemetry;
using PresenceLight.Services;
using System.Diagnostics;
using System.Threading;

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

            var services = new ServiceCollection();
            services.AddOptions();
            services.Configure<ConfigWrapper>(Configuration);
            services.AddSingleton<IGraphService, GraphService>();
            services.AddSingleton<IHueService, HueService>();
            services.AddSingleton<LIFXService, LIFXService>();
            services.AddSingleton<IYeelightService, YeelightService>();
            services.AddSingleton<ICustomApiService, CustomApiService>();
            services.AddSingleton<LIFXOAuthHelper, LIFXOAuthHelper>();
            services.AddSingleton<MainWindow>();

            DiagnosticsClient.Initialize();

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private bool IsCriticalFontLoadFailure(Exception ex)
        {
            return ex.StackTrace.Contains("MS.Internal.Text.TextInterface.FontFamily.GetFirstMatchingFont", StringComparison.OrdinalIgnoreCase) ||
                   ex.StackTrace.Contains("MS.Internal.Text.Line.Format", StringComparison.OrdinalIgnoreCase);
        }

        private void OnCriticalFontLoadFailure()
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

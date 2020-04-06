using PresenceLight.Core;
using PresenceLight.Core.Graph;
using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Deployment.Application;
using System.IO;
using Microsoft.Win32;
using PresenceLight.Telemetry;

namespace PresenceLight
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public App()
        {
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            SetAddRemoveProgramsIcon();

            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            var services = new ServiceCollection();
            services.AddOptions();
            services.Configure<ConfigWrapper>(Configuration);
            services.AddSingleton<IGraphService, GraphService>();
            services.AddSingleton<IHueService, HueService>();
            services.AddSingleton<LifxService, LifxService>();
            services.AddSingleton<MainWindow>();

            DiagnosticsClient.Initialize();

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void SetAddRemoveProgramsIcon()
        {
            if (ApplicationDeployment.IsNetworkDeployed && ApplicationDeployment.CurrentDeployment.IsFirstRun)
            {
                try
                {
                    var iconSourcePath = Path.Combine(System.Windows.Forms.Application.StartupPath, "Icons/Icon.ico");

                    if (!System.IO.File.Exists(iconSourcePath)) return;

                    var myUninstallKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
                    if (myUninstallKey == null) return;

                    var mySubKeyNames = myUninstallKey.GetSubKeyNames();
                    foreach (var subkeyName in mySubKeyNames)
                    {
                        var myKey = myUninstallKey.OpenSubKey(subkeyName, true);
                        var myValue = myKey.GetValue("DisplayName");
                        if (myValue != null && myValue.ToString() == "Presence Light") // same as in 'Product name:' field
                        {
                            myKey.SetValue("DisplayIcon", iconSourcePath);
                            break;
                        }
                    }
                }
                catch
                {
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PresenceLight.Pages
{

    public partial class AboutPage 
    {
        public AboutPage()
        {
            InitializeComponent();

            LoadAboutMe();      
        }

        public event EventHandler<RequestNavigateEventArgs>? onHyperlinkRequestNavigate;
        private void LoadAboutMe()
        {
            packageName.Text = ThisAppInfo.GetDisplayName();
            packageVersion.Text = ThisAppInfo.GetPackageVersion();
            installedFrom.Text = ThisAppInfo.GetAppInstallerUri();
            installLocation.Text = ThisAppInfo.GetInstallLocation();
            settingsLocation.Text = ThisAppInfo.GetSettingsLocation();
            installedDate.Text = ThisAppInfo.GetInstallationDate();
            RuntimeVersionInfo.Text = ThisAppInfo.GetDotNetRuntimeInfo();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var handler = onHyperlinkRequestNavigate;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}

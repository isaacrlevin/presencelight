using Microsoft.Graph;
using Microsoft.Identity.Client;
using PresenceLight.Core;
using PresenceLight.Core.Graph;
using System.Windows;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using Media = System.Windows.Media;
using System.Diagnostics;
using System.Windows.Navigation;
using PresenceLight.Core.Helpers;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;
using LifxCloud.NET.Models;
using System.Windows.Input;

namespace PresenceLight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ConfigWrapper _options;
        public ConfigWrapper Config { get; set; }
        private bool stopGraphPolling;
        private bool stopThemePolling;
        private IHueService _hueService;
        private LifxService _lifxService;
        private GraphServiceClient _graphServiceClient;
        private readonly IGraphService _graphservice;
        private WindowState lastWindowState;

        #region Init
        public MainWindow(IGraphService graphService, IHueService hueService, LifxService lifxService, IOptionsMonitor<ConfigWrapper> optionsAccessor)
        {
            InitializeComponent();

            LoadAboutMe();

            _graphservice = graphService;

            _lifxService = lifxService;
            _hueService = hueService;
            _options = optionsAccessor.CurrentValue;

            LoadSettings().ContinueWith(
        t =>
        {
            if (t.IsFaulted)
            { }

            this.Dispatcher.Invoke(() =>
            {
                LoadApp();

                var tbContext = notificationIcon.DataContext;
                DataContext = Config;
                notificationIcon.DataContext = tbContext;
            });
        });
        }

        private void LoadAboutMe()
        {
            packageName.Text = ThisAppInfo.GetDisplayName();
            assemblyVersion.Text = ThisAppInfo.GetThisAssemblyVersion();
            packageVersion.Text = ThisAppInfo.GetPackageVersion();
            installedFrom.Text = ThisAppInfo.GetAppInstallerUri();
            installLocation.Text = ThisAppInfo.GetInstallLocation();
            installedDate.Text = ThisAppInfo.GetInstallationDate();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ButtonShowRuntimeVersionInfo.Content.ToString().StartsWith("Show"))
            {
                RuntimeVersionInfo.Text = ThisAppInfo.GetDotNetRuntimeInfo();
                ButtonShowRuntimeVersionInfo.Content = "Hide Runtime Info";
            }
            else
            {
                RuntimeVersionInfo.Text = "";
                ButtonShowRuntimeVersionInfo.Content = "Show Runtime Info";
            }
        }

        private void LoadApp()
        {
            CheckHueSettings();
            CheckLifxSettings();

            if (Config.IconType == "Transparent")
            {
                Transparent.IsChecked = true;
            }
            else
            {
                White.IsChecked = true;
            }

            notificationIcon.Text = PresenceConstants.Inactive;
            notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(String.Empty, IconConstants.Inactive)));
        }

        private async Task LoadSettings()
        {
            if (!(await SettingsService.IsFilePresent()))
            {
                await SettingsService.SaveSettings(_options);
            }

            Config = await SettingsService.LoadSettings();

            if (string.IsNullOrEmpty(Config.RedirectUri))
            {
                await SettingsService.DeleteSettings();
                await SettingsService.SaveSettings(_options);
            }

            if (Config.IsPhillipsEnabled)
            {
                pnlPhillips.Visibility = Visibility.Visible;
                if (!string.IsNullOrEmpty(Config.HueApiKey))
                {
                    _options.HueApiKey = Config.HueApiKey;
                }

                if (!string.IsNullOrEmpty(Config.HueIpAddress))
                {
                    _options.HueIpAddress = Config.HueIpAddress;
                }
            }
            else
            {
                pnlPhillips.Visibility = Visibility.Collapsed;
            }

            if (Config.IsLifxEnabled)
            {
                pnlLifx.Visibility = Visibility.Visible;
                if (!string.IsNullOrEmpty(Config.HueApiKey))
                {
                    _options.LifxApiKey = Config.LifxApiKey;
                }
            }
            else
            {
                pnlLifx.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Profile Panel

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private async void CallGraphButton_Click(object sender, RoutedEventArgs e)
        {
            stopThemePolling = true;
            signInPanel.Visibility = Visibility.Collapsed;
            lblTheme.Visibility = Visibility.Collapsed;
            loadingPanel.Visibility = Visibility.Visible;
            var (profile, presence) = await System.Threading.Tasks.Task.Run(() => GetBatchContent());
            var photo = await System.Threading.Tasks.Task.Run(() => GetPhoto());

            MapUI(presence, profile, LoadImage(photo));

            if (!string.IsNullOrEmpty(Config.HueApiKey) && !string.IsNullOrEmpty(Config.HueIpAddress) && !string.IsNullOrEmpty(Config.SelectedHueLightId))
            {
                await _hueService.SetColor(presence.Availability, Config.SelectedHueLightId);
            }

            if (Config.IsLifxEnabled && !string.IsNullOrEmpty(Config.LifxApiKey))
            {
                await _lifxService.SetColor(presence.Availability, (Selector)Config.SelectedLifxItemId);
            }

            loadingPanel.Visibility = Visibility.Collapsed;
            this.signInPanel.Visibility = Visibility.Collapsed;
            hueIpAddress.IsEnabled = false;
            clientId.IsEnabled = false;
            tenantId.IsEnabled = false;
            dataPanel.Visibility = Visibility.Visible;
            while (true)
            {
                if (stopGraphPolling)
                {
                    stopGraphPolling = false;
                    notificationIcon.Text = PresenceConstants.Inactive;
                    notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(String.Empty, IconConstants.Inactive)));
                    return;
                }
                await Task.Delay(5000);
                try
                {
                    presence = await System.Threading.Tasks.Task.Run(() => GetPresence());
                    if (!string.IsNullOrEmpty(Config.HueApiKey) && !string.IsNullOrEmpty(Config.HueIpAddress) && !string.IsNullOrEmpty(Config.SelectedHueLightId))
                    {
                        await _hueService.SetColor(presence.Availability, Config.SelectedHueLightId);
                    }

                    if (Config.IsLifxEnabled && !string.IsNullOrEmpty(Config.LifxApiKey))
                    {
                        await _lifxService.SetColor(presence.Availability, (Selector)Config.SelectedLifxItemId);
                    }

                    MapUI(presence, null, null);
                }
                catch { }
            }
        }

        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            var accounts = await WPFAuthorizationProvider._application.GetAccountsAsync();
            if (accounts.Any())
            {
                try
                {
                    await WPFAuthorizationProvider._application.RemoveAsync(accounts.FirstOrDefault());
                    this.signInPanel.Visibility = Visibility.Visible;
                    dataPanel.Visibility = Visibility.Collapsed;
                    stopGraphPolling = true;

                    notificationIcon.Text = PresenceConstants.Inactive;
                    notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, IconConstants.Inactive)));
                    hueIpAddress.IsEnabled = true;
                    clientId.IsEnabled = true;
                    tenantId.IsEnabled = true;

                    if (Config.IsPhillipsEnabled && !string.IsNullOrEmpty(Config.HueApiKey) && !string.IsNullOrEmpty(Config.HueIpAddress) && !string.IsNullOrEmpty(Config.SelectedHueLightId))
                    {
                        await _hueService.SetColor("Off", Config.SelectedHueLightId);
                    }

                    if (Config.IsLifxEnabled && !string.IsNullOrEmpty(Config.LifxApiKey))
                    {
                        if (ddlLifxLights.SelectedItem.GetType() == typeof(LifxCloud.NET.Models.Group))
                        {
                            Config.SelectedHueLightId = ((LifxCloud.NET.Models.Group)ddlLifxLights.SelectedItem).Id;
                        }

                        if (ddlLifxLights.SelectedItem.GetType() == typeof(LifxCloud.NET.Models.Light))
                        {
                            Config.SelectedHueLightId = ((LifxCloud.NET.Models.Light)ddlLifxLights.SelectedItem).Id;

                        }

                        await _lifxService.SetColor("Off", (Selector)Config.SelectedLifxItemId);
                    }
                }
                catch (MsalException)
                {
                }
            }
        }
        #endregion

        #region UI Helpers
        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public Media.Color MapColor(string hexColor)
        {
            return (Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hexColor);
        }

        public void MapUI(Presence presence, User profile, BitmapImage profileImageBit)
        {
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            System.Windows.Media.Color color;
            BitmapImage image;
            switch (presence.Availability)
            {
                case "Available":
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(Config.IconType, IconConstants.Available)));
                    color = MapColor("#009933");
                    notificationIcon.Text = PresenceConstants.Available;
                    break;
                case "Busy":
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(Config.IconType, IconConstants.Busy)));
                    color = MapColor("#ff3300");
                    notificationIcon.Text = PresenceConstants.Busy;
                    break;
                case "BeRightBack":
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(Config.IconType, IconConstants.BeRightBack)));
                    color = MapColor("#ffff00");
                    notificationIcon.Text = PresenceConstants.BeRightBack;
                    break;
                case "Away":
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(Config.IconType, IconConstants.Away)));
                    color = MapColor("#ffff00");
                    notificationIcon.Text = PresenceConstants.Away;
                    break;
                case "DoNotDisturb":
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(Config.IconType, IconConstants.DoNotDisturb)));
                    color = MapColor("#800000");
                    notificationIcon.Text = PresenceConstants.DoNotDisturb;
                    break;
                case "OutOfOffice":
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(Config.IconType, IconConstants.OutOfOffice)));
                    color = MapColor("#800080");
                    notificationIcon.Text = PresenceConstants.OutOfOffice;
                    break;
                default:
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, IconConstants.Inactive)));
                    color = MapColor("#FFFFFF");
                    notificationIcon.Text = PresenceConstants.Inactive;
                    break;
            }

            if (profileImageBit != null)
            {
                profileImage.Source = profileImageBit;
            }

            notificationIcon.Icon = image;
            mySolidColorBrush.Color = color;
            status.Fill = mySolidColorBrush;
            status.StrokeThickness = 1;
            status.Stroke = System.Windows.Media.Brushes.Black;

            if (profile != null)
            {
                userName.Content = profile.DisplayName;
            }
        }
        #endregion

        #region Graph Calls
        public async Task<Presence> GetPresence()
        {
            if (!stopGraphPolling)
            {
                return await _graphServiceClient.Me.Presence.Request().GetAsync();
            }
            else
            {
                throw new Exception();
            }
        }

        public async Task<byte[]> GetPhoto()
        {
            return ReadFully(await _graphServiceClient.Me.Photo.Content.Request().GetAsync());
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public async Task<(User User, Presence Presence)> GetBatchContent()
        {
            IUserRequest userRequest = _graphServiceClient.Me.Request();
            IPresenceRequest presenceRequest = _graphServiceClient.Me.Presence.Request();

            BatchRequestContent batchRequestContent = new BatchRequestContent();

            var userRequestId = batchRequestContent.AddBatchRequestStep(userRequest);
            var presenceRequestId = batchRequestContent.AddBatchRequestStep(presenceRequest);

            BatchResponseContent returnedResponse = await _graphServiceClient.Batch.Request().PostAsync(batchRequestContent);

            User user = await returnedResponse.GetResponseByIdAsync<User>(userRequestId);
            Presence presence = await returnedResponse.GetResponseByIdAsync<Presence>(presenceRequestId);

            return (User: user, Presence: presence);
        }
        #endregion

 
        #region Settings Panel
        private async void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (Transparent.IsChecked == true)
            {
                Config.IconType = "Transparent";
            }
            else
            {
                Config.IconType = "White";
            }

            await SettingsService.SaveSettings(Config);
            lblSettingSaved.Visibility = Visibility.Visible;
            CheckHueSettings();
        }
        #endregion

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            lblHueSaved.Visibility = Visibility.Collapsed;
            lblLifxSaved.Visibility = Visibility.Collapsed;
            lblSettingSaved.Visibility = Visibility.Collapsed;
        }

        #region Tray Methods

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void OnNotifyIconDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.Show();
                this.WindowState = this.lastWindowState;
            }
        }
        private void OnOpenClick(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = this.lastWindowState;
        }
        private async void OnExitClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Config.HueApiKey) && !string.IsNullOrEmpty(Config.HueIpAddress) && !string.IsNullOrEmpty(Config.SelectedHueLightId))
            {
                await _hueService.SetColor("Off", Config.SelectedHueLightId);
            }

            if (Config.IsLifxEnabled && !string.IsNullOrEmpty(Config.LifxApiKey))
            {

                await _lifxService.SetColor("Off", (Selector)Config.SelectedLifxItemId);
            }

            System.Windows.Application.Current.Shutdown();
        }
        #endregion
    }
}
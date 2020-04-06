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
using Newtonsoft.Json;
using Windows.Storage;
using Hardcodet.Wpf.TaskbarNotification;
using System.Text.RegularExpressions;
using Q42.HueApi;
using System.Windows.Controls;

namespace PresenceLight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ConfigWrapper _options;
        public ConfigWrapper Config { get; set; }
        private bool stopPolling;
        private IHueService _hueService;
        private GraphServiceClient _graphServiceClient;
        private readonly IGraphService _graphservice;

        public MainWindow(IGraphService graphService, IHueService hueService, IOptionsMonitor<ConfigWrapper> optionsAccessor)
        {
            InitializeComponent();

            _graphservice = graphService;

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

        private void LoadApp()
        {
            CheckHueSettings();

            if (Config.IconType == "Transparent")
            {
                Transparent.IsChecked = true;
            }
            else
            {
                White.IsChecked = true;
            }

            notificationIcon.ToolTipText = PresenceConstants.Inactive;
            notificationIcon.IconSource = new BitmapImage(new Uri(IconConstants.GetIcon(String.Empty, IconConstants.Inactive)));
        }


        private async Task LoadSettings()
        {
            if (!(await SettingsService.IsFilePresent()))
            {
                await SettingsService.SaveSettings(_options);
            }

            Config = await SettingsService.LoadSettings();
            if (!string.IsNullOrEmpty(Config.HueApiKey))
            {
                _options.HueApiKey = Config.HueApiKey;
            }

            if (!string.IsNullOrEmpty(Config.HueIpAddress))
            {
                _options.HueIpAddress = Config.HueIpAddress;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private async void CallGraphButton_Click(object sender, RoutedEventArgs e)
        {
            signInPanel.Visibility = Visibility.Collapsed;
            loadingPanel.Visibility = Visibility.Visible;
            var (profile, presence) = await System.Threading.Tasks.Task.Run(() => GetBatchContent());
            var photo = await System.Threading.Tasks.Task.Run(() => GetPhoto());

            MapUI(presence, profile, LoadImage(photo));

            if (!string.IsNullOrEmpty(Config.HueApiKey) && !string.IsNullOrEmpty(Config.HueIpAddress) && ((Light)ddlLights.SelectedItem) != null)
            {
                await _hueService.SetColor(presence.Availability, ((Light)ddlLights.SelectedItem).Id);
            }

            loadingPanel.Visibility = Visibility.Collapsed;
            this.signInPanel.Visibility = Visibility.Collapsed;
            hueIpAddress.IsEnabled = false;
            clientId.IsEnabled = false;
            tenantId.IsEnabled = false;
            dataPanel.Visibility = Visibility.Visible;
            while (true)
            {
                if (stopPolling)
                {
                    stopPolling = false;
                    notificationIcon.ToolTipText = PresenceConstants.Inactive;
                    notificationIcon.IconSource = new BitmapImage(new Uri(IconConstants.GetIcon(String.Empty, IconConstants.Inactive)));
                    return;
                }
                await Task.Delay(5000);
                try
                {
                    presence = await System.Threading.Tasks.Task.Run(() => GetPresence());
                    if (!string.IsNullOrEmpty(Config.HueApiKey) && !string.IsNullOrEmpty(Config.HueIpAddress))
                    {
                        await _hueService.SetColor(presence.Availability, ((Light)ddlLights.SelectedItem).Id);
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
                    stopPolling = true;

                    notificationIcon.ToolTipText = PresenceConstants.Inactive;
                    notificationIcon.IconSource = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, IconConstants.Inactive)));
                    hueIpAddress.IsEnabled = true;
                    clientId.IsEnabled = true;
                    tenantId.IsEnabled = true;
                }
                catch (MsalException)
                {
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

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
                    notificationIcon.ToolTipText = PresenceConstants.Available;
                    break;
                case "Busy":
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(Config.IconType, IconConstants.Busy)));
                    color = MapColor("#ff3300");
                    notificationIcon.ToolTipText = PresenceConstants.Busy;
                    break;
                case "BeRightBack":
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(Config.IconType, IconConstants.BeRightBack)));
                    color = MapColor("#ffff00");
                    notificationIcon.ToolTipText = PresenceConstants.BeRightBack;
                    break;
                case "Away":
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(Config.IconType, IconConstants.Away)));
                    color = MapColor("#ffff00");
                    notificationIcon.ToolTipText = PresenceConstants.Away;
                    break;
                case "DoNotDisturb":
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(Config.IconType, IconConstants.DoNotDisturb)));
                    color = MapColor("#800000");
                    notificationIcon.ToolTipText = PresenceConstants.DoNotDisturb;
                    break;
                case "OutOfOffice":
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(Config.IconType, IconConstants.OutOfOffice)));
                    color = MapColor("#800080");
                    notificationIcon.ToolTipText = PresenceConstants.OutOfOffice;
                    break;
                default:
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, IconConstants.Inactive)));
                    color = MapColor("#FFFFFF");
                    notificationIcon.ToolTipText = PresenceConstants.Inactive;
                    break;
            }

            if (profileImageBit != null)
            {
                profileImage.Source = profileImageBit;
            }

            notificationIcon.IconSource = image;
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
            if (!stopPolling)
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

        private async void SaveHueSettings_Click(object sender, RoutedEventArgs e)
        {
            await SettingsService.SaveSettings(Config);
            _hueService = new HueService(Config);
            CheckHueSettings();
        }

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

        private async void RegisterBridge_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
            MessageBox.Show("Please press the sync button on your Phillips Hue Bridge");

            SolidColorBrush fontBrush = new SolidColorBrush();

            try
            {
                imgLoading.Visibility = Visibility.Visible;
                lblMessage.Visibility = Visibility.Collapsed;
                if (string.IsNullOrEmpty(_options.HueIpAddress))
                {
                    _options.HueIpAddress = Config.HueIpAddress;
                }
                Config.HueApiKey = await _hueService.RegisterBridge();
                ddlLights.ItemsSource = await _hueService.CheckLights();
                _options.HueApiKey = Config.HueApiKey;
                ddlLights.Visibility = Visibility.Visible;
                imgLoading.Visibility = Visibility.Collapsed;
                lblMessage.Visibility = Visibility.Visible;
                await SettingsService.SaveSettings(Config);
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error Occured registering bridge, please try again";
                fontBrush.Color = MapColor("#ff3300");
                lblMessage.Foreground = fontBrush;
            }

            if (!string.IsNullOrEmpty(Config.HueApiKey))
            {
                lblMessage.Text = "App Registered with Bridge";
                fontBrush.Color = MapColor("#009933");
                lblMessage.Foreground = fontBrush;
            }

            CheckHueSettings();
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            lblSettingSaved.Visibility = Visibility.Collapsed;
        }
        private void HueIpAddress_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (((TextBox)e.OriginalSource).Text.Trim() != ((TextBox)e.Source).Text.Trim())
            {
                if (_options != null)
                {
                    _options.HueApiKey = String.Empty;
                }
                if (Config != null)
                {
                    Config.HueApiKey = String.Empty;
                }
            }
            CheckHueSettings();
        }
        private async void CheckHueSettings()
        {
            if (Config != null)
            {
                if (!CheckAAD())
                {
                    configErrorPanel.Visibility = Visibility.Visible;
                    dataPanel.Visibility = Visibility.Hidden;
                    signInPanel.Visibility = Visibility.Hidden;
                }
                else
                {
                    configErrorPanel.Visibility = Visibility.Hidden;
                    signInPanel.Visibility = Visibility.Visible;

                    if (_graphServiceClient == null)
                    {
                        _graphServiceClient = _graphservice.GetAuthenticatedGraphClient(typeof(WPFAuthorizationProvider));
                    }
                }

                SolidColorBrush fontBrush = new SolidColorBrush();


                if (!CheckHueIp())
                {
                    lblMessage.Text = "Valid IP Address Required";
                    fontBrush.Color = MapColor("#ff3300");
                    btnRegister.IsEnabled = false;
                    ddlLights.Visibility = Visibility.Collapsed;
                    lblMessage.Foreground = fontBrush;
                }
                else
                {
                    btnRegister.IsEnabled = true;
                    if (string.IsNullOrEmpty(Config.HueApiKey))
                    {
                        lblMessage.Text = "Missing App Registration, please button on bridge than click 'Register Bridge'";
                        fontBrush.Color = MapColor("#ff3300");
                        ddlLights.Visibility = Visibility.Collapsed;
                        lblMessage.Foreground = fontBrush;
                    }
                    else
                    {
                        ddlLights.ItemsSource = await _hueService.CheckLights();
                        ddlLights.Visibility = Visibility.Visible;
                        lblMessage.Text = "App Registered with Bridge";
                        fontBrush.Color = MapColor("#009933");
                        lblMessage.Foreground = fontBrush;
                    }
                }
            }
        }
        private bool CheckHueIp()
        {
            string r1 = "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]).){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";

            string r2 = @"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b";

            Regex r = new Regex(r2);

            if (string.IsNullOrEmpty(hueIpAddress.Text.Trim()) || !r.IsMatch(hueIpAddress.Text.Trim()) || hueIpAddress.Text.Trim().EndsWith("."))
            {
                return false;
            }
            return true;
        }

        private bool CheckAAD()
        {
            Regex r = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$");
            if (string.IsNullOrEmpty(Config.ClientId) || string.IsNullOrEmpty(Config.TenantId) || string.IsNullOrEmpty(Config.RedirectUri) || !r.IsMatch(Config.ClientId) || !r.IsMatch(Config.TenantId))
            {
                return false;
            }
            return true;
        }

        private async void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            await SettingsService.SaveSettings(Config);
        }
    }
}
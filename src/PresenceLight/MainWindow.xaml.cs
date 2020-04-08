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
using System.Windows.Documents;
using LifxCloud.NET.Models;

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
        private LifxService _lifxService;
        private GraphServiceClient _graphServiceClient;
        private readonly IGraphService _graphservice;


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
            signInPanel.Visibility = Visibility.Collapsed;
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
                    stopPolling = true;

                    notificationIcon.ToolTipText = PresenceConstants.Inactive;
                    notificationIcon.IconSource = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, IconConstants.Inactive)));
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
                            Config.SelectedHueLightId = ((LifxCloud.NET.Models.Group)ddlLifxLights.SelectedItem).Id;

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

        #region Hue Panel

        private async void SaveHueSettings_Click(object sender, RoutedEventArgs e)
        {
            await SettingsService.SaveSettings(Config);
            _hueService = new HueService(Config);
            CheckHueSettings();
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
                    lblHueMessage.Text = "Valid IP Address Required";
                    fontBrush.Color = MapColor("#ff3300");
                    btnRegister.IsEnabled = false;
                    ddlHueLights.Visibility = Visibility.Collapsed;
                    lblHueMessage.Foreground = fontBrush;
                }
                else
                {
                    if (string.IsNullOrEmpty(Config.HueIpAddress))
                    {
                        Config.HueIpAddress = hueIpAddress.Text;
                    }

                    if (string.IsNullOrEmpty(_options.HueIpAddress))
                    {
                        _options.HueIpAddress = hueIpAddress.Text;
                    }
                    btnRegister.IsEnabled = true;
                    if (string.IsNullOrEmpty(Config.HueApiKey))
                    {
                        lblHueMessage.Text = "Missing App Registration, please button on bridge than click 'Register Bridge'";
                        fontBrush.Color = MapColor("#ff3300");
                        ddlHueLights.Visibility = Visibility.Collapsed;
                        lblHueMessage.Foreground = fontBrush;
                    }
                    else
                    {
                        ddlHueLights.ItemsSource = await _hueService.CheckLights();

                        foreach (var item in ddlHueLights.Items)
                        {
                            var light = (Q42.HueApi.Light)item;
                            if (light.Id == Config.SelectedHueLightId)
                            {
                                ddlHueLights.SelectedItem = item;
                            }
                        }
                        ddlHueLights.Visibility = Visibility.Visible;
                        lblHueMessage.Text = "App Registered with Bridge";
                        fontBrush.Color = MapColor("#009933");
                        lblHueMessage.Foreground = fontBrush;
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

            _options.ClientId = Config.ClientId;
            _options.TenantId = Config.TenantId;
            _options.RedirectUri = Config.RedirectUri;

            return true;
        }

        private async void FindBridge_Click(object sender, RoutedEventArgs e)
        {
            hueIpAddress.Text = await _hueService.FindBridge();
        }

        private void cbIsPhillipsEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Config.IsPhillipsEnabled)
            {
                pnlPhillips.Visibility = Visibility.Visible;
            }
            else
            {
                pnlPhillips.Visibility = Visibility.Collapsed;
            }
        }

        private async void RegisterBridge_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
            MessageBox.Show("Please press the sync button on your Phillips Hue Bridge");

            SolidColorBrush fontBrush = new SolidColorBrush();

            try
            {
                imgLoading.Visibility = Visibility.Visible;
                lblHueMessage.Visibility = Visibility.Collapsed;
                if (string.IsNullOrEmpty(_options.HueIpAddress))
                {
                    _options.HueIpAddress = Config.HueIpAddress;
                }
                Config.HueApiKey = await _hueService.RegisterBridge();
                ddlHueLights.ItemsSource = await _hueService.CheckLights();
                _options.HueApiKey = Config.HueApiKey;
                ddlHueLights.Visibility = Visibility.Visible;
                imgLoading.Visibility = Visibility.Collapsed;
                lblHueMessage.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                lblHueMessage.Text = "Error Occured registering bridge, please try again";
                fontBrush.Color = MapColor("#ff3300");
                lblHueMessage.Foreground = fontBrush;
            }

            if (!string.IsNullOrEmpty(Config.HueApiKey))
            {
                lblHueMessage.Text = "App Registered with Bridge";
                fontBrush.Color = MapColor("#009933");
                lblHueMessage.Foreground = fontBrush;
            }

            CheckHueSettings();
        }

        #endregion

        #region Lifx Panel

        private async void SaveLifxSettings_Click(object sender, RoutedEventArgs e)
        {
            await SettingsService.SaveSettings(Config);
        }

        private async void CheckLifx_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush fontBrush = new SolidColorBrush();

            if (!string.IsNullOrEmpty(lifxApiKey.Text))
            {
                try
                {
                    _options.LifxApiKey = lifxApiKey.Text;
                    Config.LifxApiKey = lifxApiKey.Text;

                    if (((System.Windows.Controls.Button)sender).Name == "btnGetLifxGroups")
                    {
                        ddlLifxLights.ItemsSource = await _lifxService.GetAllGroupsAsync();
                    }
                    else
                    {
                        ddlLifxLights.ItemsSource = await _lifxService.GetAllLightsAsync();
                    }

                    ddlLifxLights.Visibility = Visibility.Visible;
                    lblLifxMessage.Text = "Connected to Lifx Cloud";
                    fontBrush.Color = MapColor("#009933");
                    lblLifxMessage.Foreground = fontBrush;
                }
                catch
                {
                    ddlLifxLights.Visibility = Visibility.Collapsed;
                    lblLifxMessage.Text = "Error Occured Connecting to Lifx, please try again";
                    fontBrush.Color = MapColor("#ff3300");
                    lblLifxMessage.Foreground = fontBrush;
                }
            }
            else
            {

                Run run1 = new Run("Valid Lifx Key Required ");
                Run run2 = new Run(" https://cloud.lifx.com/settings");

                Hyperlink hyperlink = new Hyperlink(run2)
                {
                    NavigateUri = new Uri("https://cloud.lifx.com/settings")
                };
                hyperlink.RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler(Hyperlink_RequestNavigate); //to be implemented
                lblLifxMessage.Inlines.Clear();
                lblLifxMessage.Inlines.Add(run1);
                lblLifxMessage.Inlines.Add(hyperlink);


                fontBrush.Color = MapColor("#ff3300");
                lblLifxMessage.Foreground = fontBrush;

            }
        }

        private void cbIsLifxEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Config.IsLifxEnabled)
            {
                pnlLifx.Visibility = Visibility.Visible;
            }
            else
            {
                pnlLifx.Visibility = Visibility.Collapsed;
            }
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

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _hueService.SetColor("Off", Config.SelectedHueLightId);
            _lifxService.SetColor("Off", (Selector)Config.SelectedLifxItemId);
            this.Hide();
            e.Cancel = true;
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            lblSettingSaved.Visibility = Visibility.Collapsed;
        }

        private void ddlHueLights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddlHueLights.SelectedItem != null)
            {
                Config.SelectedHueLightId = ((Q42.HueApi.Light)ddlHueLights.SelectedItem).Id;
                _options.SelectedHueLightId = Config.SelectedHueLightId;
            }
        }

        private void ddlLifxLights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddlLifxLights.SelectedItem != null)
            {
                // Get whether item is group or light
                if (ddlLifxLights.SelectedItem.GetType() == typeof(LifxCloud.NET.Models.Group))
                {
                    Config.SelectedLifxItemId = $"group_id:{((LifxCloud.NET.Models.Group)ddlLifxLights.SelectedItem).Id}";
                }

                if (ddlLifxLights.SelectedItem.GetType() == typeof(LifxCloud.NET.Models.Light))
                {
                    Config.SelectedLifxItemId = $"id:{((LifxCloud.NET.Models.Light)ddlLifxLights.SelectedItem).Id}";

                }
                _options.SelectedLifxItemId = Config.SelectedLifxItemId;

            }
        }
    }
}
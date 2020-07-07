using System;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using PresenceLight.Core;
using PresenceLight.Core.Graph;
using System.Windows;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using Media = System.Windows.Media;
using System.Windows.Navigation;
using System.Text.RegularExpressions;
using LifxCloud.NET.Models;
using System.Windows.Input;
using PresenceLight.Services;
using PresenceLight.Core.Services;
using System.Reflection;
using PresenceLight.Telemetry;
using System.Reflection.PortableExecutable;
using System.Collections.Generic;

namespace PresenceLight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ConfigWrapper _options;
        public ConfigWrapper Config { get; set; }

        private string lightMode;

        private Presence presence { get; set; }
        private DateTime settingsLastSaved = DateTime.MinValue;

        private IYeelightService _yeelightService;
        private IHueService _hueService;
        private ICustomApiService _customApiService;
        private LIFXOAuthHelper _lIFXOAuthHelper;
        private LIFXService _lifxService;
        private GraphServiceClient _graphServiceClient;
        private readonly IGraphService _graphservice;
        private WindowState lastWindowState;

        #region Init
        public MainWindow(IGraphService graphService, IHueService hueService, LIFXService lifxService, IYeelightService yeelightService, ICustomApiService customApiService, IOptionsMonitor<ConfigWrapper> optionsAccessor, LIFXOAuthHelper lifxOAuthHelper)
        {
            InitializeComponent();

            System.Windows.Application.Current.SessionEnding += new SessionEndingCancelEventHandler(Current_SessionEnding);

            LoadAboutMe();

            _graphservice = graphService;
            _yeelightService = yeelightService;
            _lifxService = lifxService;
            _hueService = hueService;
            _customApiService = customApiService;
            _options = optionsAccessor.CurrentValue;
            _lIFXOAuthHelper = lifxOAuthHelper;
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
            packageVersion.Text = ThisAppInfo.GetPackageVersion();
            installedFrom.Text = ThisAppInfo.GetAppInstallerUri();
            installLocation.Text = ThisAppInfo.GetInstallLocation();
            installedDate.Text = ThisAppInfo.GetInstallationDate();
            RuntimeVersionInfo.Text = ThisAppInfo.GetDotNetRuntimeInfo();
        }

        private void LoadApp()
        {
            CheckHue();
            CheckLIFX();
            CheckYeelight();
            CheckAAD();

            if (Config.IconType == "Transparent")
            {
                Transparent.IsChecked = true;
            }
            else
            {
                White.IsChecked = true;
            }

            PopulateWorkingDays();

            notificationIcon.Text = PresenceConstants.Inactive;
            notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(String.Empty, IconConstants.Inactive)));

            turnOffButton.Visibility = Visibility.Collapsed;
            turnOnButton.Visibility = Visibility.Collapsed;

            CallGraph();
        }

        private void SyncOptions()
        {
            PropertyInfo[] properties = typeof(ConfigWrapper).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(Config);
                property.SetValue(_options, value);
            }
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
            if (Config.LightSettings.UseWorkingHours)
            {
                pnlWorkingHours.Visibility = Visibility.Visible;
                SyncOptions();
            }
            else
            {
                pnlWorkingHours.Visibility = Visibility.Collapsed;
                SyncOptions();
            }

            if (Config.LightSettings.Hue.IsPhillipsHueEnabled)
            {
                pnlPhillips.Visibility = Visibility.Visible;
                SyncOptions();
            }
            else
            {
                pnlPhillips.Visibility = Visibility.Collapsed;
            }

            if (Config.LightSettings.Yeelight.IsYeelightEnabled)
            {
                pnlYeelight.Visibility = Visibility.Visible;
                SyncOptions();
            }
            else
            {
                pnlYeelight.Visibility = Visibility.Collapsed;
            }

            if (Config.LightSettings.LIFX.IsLIFXEnabled)
            {
                getTokenLink.Visibility = Visibility.Visible;
                pnlLIFX.Visibility = Visibility.Visible;

                SyncOptions();
            }
            else
            {
                getTokenLink.Visibility = Visibility.Collapsed;
                pnlLIFX.Visibility = Visibility.Collapsed;
            }

            if (Config.LightSettings.Custom.IsCustomApiEnabled)
            {
                pnlCustomApi.Visibility = Visibility.Visible;
                customApiAvailableMethod.SelectedValue = Config.LightSettings.Custom.CustomApiAvailableMethod;
                customApiBusyMethod.SelectedValue = Config.LightSettings.Custom.CustomApiBusyMethod;
                customApiBeRightBackMethod.SelectedValue = Config.LightSettings.Custom.CustomApiBeRightBackMethod;
                customApiAwayMethod.SelectedValue = Config.LightSettings.Custom.CustomApiAwayMethod;
                customApiDoNotDisturbMethod.SelectedValue = Config.LightSettings.Custom.CustomApiDoNotDisturbMethod;
                customApiOfflineMethod.SelectedValue = Config.LightSettings.Custom.CustomApiOfflineMethod;
                customApiOffMethod.SelectedValue = Config.LightSettings.Custom.CustomApiOffMethod;
                SyncOptions();
            }
            else
            {
                pnlCustomApi.Visibility = Visibility.Collapsed;
            }
            if (!string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXClientId) && !(string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXClientSecret)))
            {
                getTokenLink.Visibility = Visibility.Visible;
            }
            else
            {
                getTokenLink.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Profile Panel

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var url = e.Uri.AbsoluteUri;
            Helpers.OpenBrowser(url);
            e.Handled = true;
        }

        private async void SignIn_Click(object sender, RoutedEventArgs e)
        {
            await CallGraph();
        }

        private async Task CallGraph()
        {
            lightMode = "Graph";
            syncTeamsButton.IsEnabled = false;
            syncThemeButton.IsEnabled = true;

            if (_graphServiceClient == null)
            {
                _graphServiceClient = _graphservice.GetAuthenticatedGraphClient(typeof(WPFAuthorizationProvider));
            }

            signInPanel.Visibility = Visibility.Collapsed;
            lblTheme.Visibility = Visibility.Collapsed;
            loadingPanel.Visibility = Visibility.Visible;
            var (profile, presence) = await System.Threading.Tasks.Task.Run(() => GetBatchContent());
            var photo = await System.Threading.Tasks.Task.Run(() => GetPhoto());

            if (photo == null)
            {
                MapUI(presence, profile, new BitmapImage(new Uri("pack://application:,,,/PresenceLight;component/images/UnknownProfile.png")));
            }
            else
            {
                MapUI(presence, profile, LoadImage(photo));
            }

            if (lightMode == "Graph")
            {
                await SetColor(presence.Availability);
            }
            loadingPanel.Visibility = Visibility.Collapsed;
            this.signInPanel.Visibility = Visibility.Collapsed;
            hueIpAddress.IsEnabled = false;

            dataPanel.Visibility = Visibility.Visible;
            await SettingsService.SaveSettings(Config);

            turnOffButton.Visibility = Visibility.Visible;
            turnOnButton.Visibility = Visibility.Collapsed;

            await InteractWithLights();
        }

        public async Task SetColor(string color)
        {
            if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedHueLightId))
            {
                await _hueService.SetColor(color, Config.LightSettings.Hue.SelectedHueLightId);
            }

            if (Config.LightSettings.LIFX.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey))
            {
                await _lifxService.SetColor(color, (Selector)Config.LightSettings.LIFX.SelectedLIFXItemId);
            }

            if (Config.LightSettings.Yeelight.IsYeelightEnabled && !string.IsNullOrEmpty(Config.LightSettings.Yeelight.SelectedYeelightId))
            {
                await _yeelightService.SetColor(color, Config.LightSettings.Yeelight.SelectedYeelightId);
            }

            if (Config.LightSettings.Custom.IsCustomApiEnabled)
            {
                string response = await _customApiService.SetColor(color);
                customApiLastResponse.Content = response;
                if (response.StartsWith("Error:"))
                {
                    customApiLastResponse.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    customApiLastResponse.Foreground = new SolidColorBrush(Colors.Green);
                }
            }
        }

        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            lightMode = "Graph";
            var accounts = await WPFAuthorizationProvider.Application.GetAccountsAsync();
            if (accounts.Any())
            {
                try
                {
                    await WPFAuthorizationProvider.Application.RemoveAsync(accounts.FirstOrDefault());
                    this.signInPanel.Visibility = Visibility.Visible;
                    dataPanel.Visibility = Visibility.Collapsed;

                    notificationIcon.Text = PresenceConstants.Inactive;
                    notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, IconConstants.Inactive)));
                    hueIpAddress.IsEnabled = true;

                    if (Config.LightSettings.Hue.IsPhillipsHueEnabled && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedHueLightId))
                    {
                        await _hueService.SetColor("Off", Config.LightSettings.Hue.SelectedHueLightId);
                    }

                    if (lightMode == "Graph")
                    {
                        await SetColor("Off");
                    }
                }
                catch (MsalException)
                {
                }
            }
            await SettingsService.SaveSettings(Config);
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
                    color = MapColor("#B03CDE");
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

            activity.Content = "Activity: " + presence.Activity;
            availability.Content = "Availability: " + presence.Availability;
        }
        #endregion

        #region Graph Calls
        public async Task<Presence> GetPresence()
        {
            return await _graphServiceClient.Me.Presence.Request().GetAsync();
        }

        public async Task<byte[]> GetPhoto()
        {
            try
            {
                var photo = await _graphServiceClient.Me.Photo.Content.Request().GetAsync();

                if (photo == null)
                {
                    return null;
                }
                else
                {
                    return ReadFully(photo);
                }
            }
            catch
            {
                return null;
            }
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
            btnSettings.IsEnabled = false;
            if (Transparent.IsChecked == true)
            {
                Config.IconType = "Transparent";
            }
            else
            {
                Config.IconType = "White";
            }

            CheckAAD();
            Config.LightSettings.DefaultBrightness = Convert.ToInt32(brightness.Value);

            SetWorkingDays();

            SyncOptions();
            await SettingsService.SaveSettings(Config);
            lblSettingSaved.Visibility = Visibility.Visible;
            btnSettings.IsEnabled = true;
        }

        private void SetWorkingDays()
        {
            List<string> days = new List<string>();

            if (Monday.IsChecked.Value)
            {
                days.Add("Monday");
            }

            if (Tuesday.IsChecked.Value)
            {
                days.Add("Tuesday");
            }

            if (Wednesday.IsChecked.Value)
            {
                days.Add("Wednesday");
            }

            if (Thursday.IsChecked.Value)
            {
                days.Add("Thursday");
            }

            if (Friday.IsChecked.Value)
            {
                days.Add("Friday");
            }

            if (Saturday.IsChecked.Value)
            {
                days.Add("Saturday");
            }

            if (Sunday.IsChecked.Value)
            {
                days.Add("Sunday");
            }

            Config.LightSettings.WorkingDays = string.Join("|", days);
        }

        private void CheckAAD()
        {
            Regex r = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$");
            if (string.IsNullOrEmpty(Config.ClientId) || string.IsNullOrEmpty(Config.RedirectUri) || !r.IsMatch(Config.ClientId))
            {
                configErrorPanel.Visibility = Visibility.Visible;
                dataPanel.Visibility = Visibility.Hidden;
                signInPanel.Visibility = Visibility.Hidden;
                return;
            }

            SyncOptions();

            configErrorPanel.Visibility = Visibility.Hidden;

            if (dataPanel.Visibility != Visibility.Visible)
            {
                signInPanel.Visibility = Visibility.Visible;
            }

            if (_graphServiceClient == null)
            {
                _graphServiceClient = _graphservice.GetAuthenticatedGraphClient(typeof(WPFAuthorizationProvider));
            }
        }
        #endregion

        private void PopulateWorkingDays()
        {
            if (!string.IsNullOrEmpty(Config.LightSettings.WorkingDays))
            {

                if (Config.LightSettings.WorkingDays.Contains("Monday"))
                {
                    Monday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Tuesday"))
                {
                    Tuesday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Wednesday"))
                {
                    Wednesday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Thursday"))
                {
                    Thursday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Friday"))
                {
                    Friday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Saturday"))
                {
                    Saturday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Sunday"))
                {
                    Sunday.IsChecked = true;
                }
            }
        }


        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            lblHueSaved.Visibility = Visibility.Collapsed;
            lblLIFXSaved.Visibility = Visibility.Collapsed;
            lblSettingSaved.Visibility = Visibility.Collapsed;
        }

        private async void cbSyncLights(object sender, RoutedEventArgs e)
        {
            if (!Config.LightSettings.SyncLights)
            {
                await SetColor("Off");
                turnOffButton.Visibility = Visibility.Collapsed;
                turnOnButton.Visibility = Visibility.Visible;
            }

            SyncOptions();
            await SettingsService.SaveSettings(Config);
            e.Handled = true;
        }

        private async void cbUseDefaultBrightnessChanged(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.UseDefaultBrightness)
            {
                pnlDefaultBrightness.Visibility = Visibility.Visible;
            }
            else
            {
                pnlDefaultBrightness.Visibility = Visibility.Collapsed;
            }

            SyncOptions();
            await SettingsService.SaveSettings(Config);
            e.Handled = true;
        }

        private void cbUseWorkingHoursChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Config.LightSettings.WorkingHoursStartTime))
            {
                Config.LightSettings.WorkingHoursStartTime = DateTime.Parse(Config.LightSettings.WorkingHoursStartTime).TimeOfDay.ToString();
            }

            if (!string.IsNullOrEmpty(Config.LightSettings.WorkingHoursEndTime))
            {
                Config.LightSettings.WorkingHoursEndTime = DateTime.Parse(Config.LightSettings.WorkingHoursEndTime).TimeOfDay.ToString();
            }

            if (Config.LightSettings.UseWorkingHours)
            {
                pnlWorkingHours.Visibility = Visibility.Visible;
            }
            else
            {
                pnlWorkingHours.Visibility = Visibility.Collapsed;
            }

            SyncOptions();
            e.Handled = true;
        }

        #region Tray Methods

        protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            await SettingsService.SaveSettings(Config);
            this.Hide();
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

        private void OnTurnOnSyncClick(object sender, RoutedEventArgs e)
        {
            lightMode = "Graph";

            turnOffButton.Visibility = Visibility.Visible;
            turnOnButton.Visibility = Visibility.Collapsed;

            this.WindowState = this.lastWindowState;
        }

        private async void OnTurnOffSyncClick(object sender, RoutedEventArgs e)
        {
            lightMode = "Custom";

            if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedHueLightId))
            {
                await _hueService.SetColor("Off", Config.LightSettings.Hue.SelectedHueLightId);
            }

            if (Config.LightSettings.LIFX.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey))
            {

                await _lifxService.SetColor("Off", (Selector)Config.LightSettings.LIFX.SelectedLIFXItemId);
            }

            turnOffButton.Visibility = Visibility.Collapsed;
            turnOnButton.Visibility = Visibility.Visible;

            notificationIcon.Text = PresenceConstants.Inactive;
            notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, IconConstants.Inactive)));

            this.WindowState = this.lastWindowState;
        }


        private async void OnExitClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedHueLightId))
            {
                await _hueService.SetColor("Off", Config.LightSettings.Hue.SelectedHueLightId);
            }

            if (Config.LightSettings.LIFX.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey))
            {

                await _lifxService.SetColor("Off", (Selector)Config.LightSettings.LIFX.SelectedLIFXItemId);
            }
            await SettingsService.SaveSettings(Config);
            System.Windows.Application.Current.Shutdown();
        }
        private async void Current_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedHueLightId))
            {
                await _hueService.SetColor("Off", Config.LightSettings.Hue.SelectedHueLightId);
            }

            if (Config.LightSettings.LIFX.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey))
            {

                await _lifxService.SetColor("Off", (Selector)Config.LightSettings.LIFX.SelectedLIFXItemId);
            }

            if (Config.LightSettings.Custom.IsCustomApiEnabled && !string.IsNullOrEmpty(Config.LightSettings.Custom.CustomApiOffMethod) && !string.IsNullOrEmpty(Config.LightSettings.Custom.CustomApiOffUri))
            {
                await _customApiService.SetColor("Off");
            }

            await SettingsService.SaveSettings(Config);
        }
        #endregion

        bool IsInWorkingHours()
        {
            if (string.IsNullOrEmpty(Config.LightSettings.WorkingHoursStartTime) || string.IsNullOrEmpty(Config.LightSettings.WorkingHoursEndTime) || string.IsNullOrEmpty(Config.LightSettings.WorkingDays))
            {
                return false;
            }

            if (!Config.LightSettings.WorkingDays.Contains(DateTime.Now.DayOfWeek.ToString()))
            {
                return false;
            }

            // convert datetime to a TimeSpan
            bool validStart = TimeSpan.TryParse(Config.LightSettings.WorkingHoursStartTime, out TimeSpan start);
            bool validEnd = TimeSpan.TryParse(Config.LightSettings.WorkingHoursEndTime, out TimeSpan end);
            if (!validEnd || !validStart)
            {
                return false;
            }

            TimeSpan now = DateTime.Now.TimeOfDay;
            // see if start comes before end
            if (start < end)
                return start <= now && now <= end;
            // start is after end, so do the inverse comparison
            return !(end < now && now < start);
        }

        private void time_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!string.IsNullOrEmpty(Config.LightSettings.WorkingHoursStartTime))
            {
                Config.LightSettings.WorkingHoursStartTime = DateTime.Parse(Config.LightSettings.WorkingHoursStartTime).TimeOfDay.ToString();
            }

            if (!string.IsNullOrEmpty(Config.LightSettings.WorkingHoursEndTime))
            {
                Config.LightSettings.WorkingHoursEndTime = DateTime.Parse(Config.LightSettings.WorkingHoursEndTime).TimeOfDay.ToString();
            }

            SyncOptions();
            e.Handled = true;
        }

        private async Task InteractWithLights()
        {
            while (true)
            {
                await Task.Delay(Convert.ToInt32(Config.LightSettings.PollingInterval * 1000));

                if (Config.LightSettings.SyncLights)
                {
                    if (!Config.LightSettings.UseWorkingHours || (Config.LightSettings.UseWorkingHours && IsInWorkingHours()))
                    {
                        switch (lightMode)
                        {
                            case "Graph":
                                try
                                {
                                    presence = await System.Threading.Tasks.Task.Run(() => GetPresence());

                                    await SetColor(presence.Availability);

                                    if (DateTime.Now.AddMinutes(-5) > settingsLastSaved)
                                    {
                                        await SettingsService.SaveSettings(Config);
                                        settingsLastSaved = DateTime.Now;
                                    }

                                    MapUI(presence, null, null);

                                }
                                catch (Exception e)
                                {
                                    DiagnosticsClient.TrackException(e);
                                }

                                break;
                            case "Theme":

                                try
                                {
                                    var theme = ((SolidColorBrush)SystemParameters.WindowGlassBrush).Color;
                                    var color = $"#{theme.ToString().Substring(3)}";

                                    lblTheme.Content = $"Theme Color is {color}";
                                    lblTheme.Foreground = (SolidColorBrush)SystemParameters.WindowGlassBrush;
                                    lblTheme.Visibility = Visibility.Visible;

                                    if (lightMode == "Theme")
                                    {
                                        await SetColor(color);
                                    }

                                    if (DateTime.Now.Minute % 5 == 0)
                                    {
                                        await SettingsService.SaveSettings(Config);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    DiagnosticsClient.TrackException(ex);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
    }
}

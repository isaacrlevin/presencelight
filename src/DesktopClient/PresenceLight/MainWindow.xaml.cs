using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using LifxCloud.NET.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;

using PresenceLight.Core;
using PresenceLight.Graph;
using PresenceLight.Services;
using PresenceLight.Telemetry;

using Media = System.Windows.Media;

namespace PresenceLight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BaseConfig _options;
        public BaseConfig Config { get; set; }

        private string lightMode;

        private Presence presence { get; set; }
        private DateTime settingsLastSaved = DateTime.MinValue;

        private IYeelightService _yeelightService;
        private IHueService _hueService;
        private IRemoteHueService _remoteHueService;
        private ICustomApiService _customApiService;
        private LIFXOAuthHelper _lIFXOAuthHelper;
        private LIFXService _lifxService;
        private GraphServiceClient _graphServiceClient;
        private readonly IGraphService _graphservice;
        private DiagnosticsClient _diagClient;
        private ISettingsService _settingsService;
        private WindowState lastWindowState;
        private bool IsWorkingHours;
        private bool previousRemoteFlag;
        private readonly ILogger<MainWindow> _logger;

        #region Init
        public MainWindow(IGraphService graphService, IHueService hueService, LIFXService lifxService, IYeelightService yeelightService, IRemoteHueService remoteHueService,
            ICustomApiService customApiService, IOptionsMonitor<BaseConfig> optionsAccessor, LIFXOAuthHelper lifxOAuthHelper, DiagnosticsClient diagClient, ILogger<MainWindow> logger,
            ISettingsService settingsService)
        {
            _logger = logger;
            InitializeComponent();

            System.Windows.Application.Current.SessionEnding += new SessionEndingCancelEventHandler(Current_SessionEnding);

            LoadAboutMe();

            _graphservice = graphService;
            _yeelightService = yeelightService;
            _lifxService = lifxService;
            _hueService = hueService;
            _remoteHueService = remoteHueService;
            _customApiService = customApiService;
            _options = optionsAccessor != null ? optionsAccessor.CurrentValue : throw new NullReferenceException("Options Accessor is null");
            _lIFXOAuthHelper = lifxOAuthHelper;
            _diagClient = diagClient;
            _settingsService = settingsService;

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
        }, TaskScheduler.Current);
        }

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

        private void LoadApp()
        {
            try
            {
                CheckHue(true);
                CheckLIFX();
                CheckYeelight();
                CheckAAD();

                previousRemoteFlag = Config.LightSettings.Hue.UseRemoteApi;

                if (Config.IconType == "Transparent")
                {
                    Transparent.IsChecked = true;
                }
                else
                {
                    White.IsChecked = true;
                }

                switch (Config.LightSettings.HoursPassedStatus)
                {
                    case "Keep":
                        HourStatusKeep.IsChecked = true;
                        break;
                    case "White":
                        HourStatusWhite.IsChecked = true;
                        break;
                    case "Off":
                        HourStatusOff.IsChecked = true;
                        break;
                    default:
                        HourStatusKeep.IsChecked = true;
                        break;
                }

                PopulateWorkingDays();

                notificationIcon.Text = PresenceConstants.Inactive;
                notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(String.Empty, IconConstants.Inactive)));

                turnOffButton.Visibility = Visibility.Collapsed;
                turnOnButton.Visibility = Visibility.Collapsed;

                Config.LightSettings.WorkingHoursStartTimeAsDate = string.IsNullOrEmpty(Config.LightSettings.WorkingHoursStartTime) ? null : DateTime.Parse(Config.LightSettings.WorkingHoursStartTime, null);
                Config.LightSettings.WorkingHoursEndTimeAsDate = string.IsNullOrEmpty(Config.LightSettings.WorkingHoursEndTime) ? null : DateTime.Parse(Config.LightSettings.WorkingHoursEndTime, null);


                CallGraph().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error occured", e);
                _diagClient.TrackException(e);
            }
        }

        private void SyncOptions()
        {
            PropertyInfo[] properties = typeof(BaseConfig).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(Config);

                if (property.PropertyType == typeof(string) && value != null && string.IsNullOrEmpty(value.ToString()))
                {
                    property.SetValue(_options, value.ToString().Trim());
                }
                else
                {
                    property.SetValue(_options, value);
                }
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
            await CallGraph().ConfigureAwait(true);
        }

        private async Task CallGraph()
        {

            lightMode = "Graph";
            syncTeamsButton.IsEnabled = false;
            syncThemeButton.IsEnabled = true;

            if (_graphServiceClient == null)
            {
                _graphServiceClient = _graphservice.GetAuthenticatedGraphClient();
            }

            signInPanel.Visibility = Visibility.Collapsed;
            lblTheme.Visibility = Visibility.Collapsed;
            loadingPanel.Visibility = Visibility.Visible;

            try
            {
                var (profile, presence) = await System.Threading.Tasks.Task.Run(() => GetBatchContent()).ConfigureAwait(true);
                var photo = await System.Threading.Tasks.Task.Run(() => GetPhoto()).ConfigureAwait(true);

                if (photo == null)
                {
                    MapUI(presence, profile, new BitmapImage(new Uri("pack://application:,,,/PresenceLight;component/images/UnknownProfile.png")));
                }
                else
                {
                    MapUI(presence, profile, LoadImage(photo));
                }


                if (Config.LightSettings.SyncLights)
                {
                    if (!Config.LightSettings.UseWorkingHours)
                    {
                        if (lightMode == "Graph")
                        {
                            await SetColor(presence.Availability, presence.Activity).ConfigureAwait(true);
                        }
                    }
                    else
                    {
                        bool previousWorkingHours = IsWorkingHours;
                        if (IsInWorkingHours())
                        {
                            if (lightMode == "Graph")
                            {
                                await SetColor(presence.Availability, presence.Activity).ConfigureAwait(true);
                            }
                        }
                        else
                        {
                            // check to see if working hours have passed
                            if (previousWorkingHours)
                            {
                                if (lightMode == "Graph")
                                {
                                    switch (Config.LightSettings.HoursPassedStatus)
                                    {
                                        case "Keep":
                                            await SetColor(presence.Availability, presence.Activity).ConfigureAwait(true);
                                            break;
                                        case "White":
                                            await SetColor("Offline", presence.Activity).ConfigureAwait(true);
                                            break;
                                        case "Off":
                                            await SetColor("Off", presence.Activity).ConfigureAwait(true);
                                            break;
                                        default:
                                            await SetColor(presence.Availability, presence.Activity).ConfigureAwait(true);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }

                loadingPanel.Visibility = Visibility.Collapsed;
                this.signInPanel.Visibility = Visibility.Collapsed;


                dataPanel.Visibility = Visibility.Visible;
                await _settingsService.SaveSettings(Config).ConfigureAwait(true);

                turnOffButton.Visibility = Visibility.Visible;
                turnOnButton.Visibility = Visibility.Collapsed;

                await InteractWithLights().ConfigureAwait(true);
            }

            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error occured", e);
                _diagClient.TrackException(e);
            }
        }

        public async Task SetColor(string color, string? activity = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedHueLightId))
                {
                    if (Config.LightSettings.Hue.UseRemoteApi)
                    {
                        if (!string.IsNullOrEmpty(Config.LightSettings.Hue.RemoteBridgeId))
                        {
                            await _remoteHueService.SetColor(color, Config.LightSettings.Hue.SelectedHueLightId, Config.LightSettings.Hue.RemoteBridgeId).ConfigureAwait(true);
                        }
                    }
                    else
                    {
                        await _hueService.SetColor(color, Config.LightSettings.Hue.SelectedHueLightId).ConfigureAwait(true);
                    }
                }

                if (Config.LightSettings.LIFX.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey))
                {
                    await _lifxService.SetColor(color, Config.LightSettings.LIFX.SelectedLIFXItemId).ConfigureAwait(true);
                }

                if (Config.LightSettings.Yeelight.IsYeelightEnabled && !string.IsNullOrEmpty(Config.LightSettings.Yeelight.SelectedYeelightId))
                {
                    await _yeelightService.SetColor(color, Config.LightSettings.Yeelight.SelectedYeelightId).ConfigureAwait(true);
                }

                if (Config.LightSettings.Custom.IsCustomApiEnabled)
                {
                    string response = await _customApiService.SetColor(color, activity).ConfigureAwait(true);
                    customApiLastResponse.Content = response;
                    if (response.Contains("Error:", StringComparison.OrdinalIgnoreCase))
                    {
                        customApiLastResponse.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        customApiLastResponse.Foreground = new SolidColorBrush(Colors.Green);
                    }
                }
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error Occured", e);
                _diagClient.TrackException(e);
                throw;
            }
        }

        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            Helpers.AppendLogger(_logger, "Signing out of Graph PresenceLight Sync");
            lightMode = "Graph";
            var accounts = await WPFAuthorizationProvider.Application.GetAccountsAsync().ConfigureAwait(true);
            if (accounts.Any())
            {
                try
                {
                    await WPFAuthorizationProvider.Application.RemoveAsync(accounts.FirstOrDefault()).ConfigureAwait(true);
                    this.signInPanel.Visibility = Visibility.Visible;
                    dataPanel.Visibility = Visibility.Collapsed;

                    notificationIcon.Text = PresenceConstants.Inactive;
                    notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, IconConstants.Inactive)));

                    if (Config.LightSettings.Hue.IsPhillipsHueEnabled && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedHueLightId))
                    {
                        if (Config.LightSettings.Hue.UseRemoteApi)
                        {
                            await _remoteHueService.SetColor("Off", Config.LightSettings.Hue.SelectedHueLightId, Config.LightSettings.Hue.RemoteBridgeId).ConfigureAwait(true);
                        }
                        else
                        {
                            await _hueService.SetColor("Off", Config.LightSettings.Hue.SelectedHueLightId).ConfigureAwait(true);
                        }
                    }

                    if (lightMode == "Graph")
                    {
                        await SetColor("Off").ConfigureAwait(true);
                    }
                }
                catch (MsalException)
                {
                }
            }
            await _settingsService.SaveSettings(Config).ConfigureAwait(true);
        }
        #endregion

        #region UI Helpers
        private BitmapImage? LoadImage(byte[] imageData)
        {
            try
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
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error Occured in LoadImager", e);
                _diagClient.TrackException(e);
                throw;
            }
        }

        public Media.Color MapColor(string hexColor)
        {
            return (Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hexColor);
        }

        public void MapUI(Presence presence, User profile, BitmapImage profileImageBit)
        {
            try
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
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error Occured", e);
                _diagClient.TrackException(e);
                throw;
            }
        }
        #endregion

        #region Graph Calls
        public async Task<Presence> GetPresence()
        {
            try
            {
                return await _graphServiceClient.Me.Presence.Request().GetAsync().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error Occured", e);
                _diagClient.TrackException(e);
                throw;
            }
        }

        public async Task<byte[]?> GetPhoto()
        {
            try
            {
                var photo = await _graphServiceClient.Me.Photo.Content.Request().GetAsync().ConfigureAwait(true);

                if (photo == null)
                {
                    return null;
                }
                else
                {
                    return ReadFully(photo);
                }
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error Occured", e);
                _diagClient.TrackException(e);
                throw;
            }
        }

        public byte[] ReadFully(Stream input)
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
            Helpers.AppendLogger(_logger, "Getting Graph Data: Profle, Image, Presence");
            try
            {
                IUserRequest userRequest = _graphServiceClient.Me.Request();
                IPresenceRequest presenceRequest = _graphServiceClient.Me.Presence.Request();

                BatchRequestContent batchRequestContent = new BatchRequestContent();

                var userRequestId = batchRequestContent.AddBatchRequestStep(userRequest);
                var presenceRequestId = batchRequestContent.AddBatchRequestStep(presenceRequest);

                BatchResponseContent returnedResponse = await _graphServiceClient.Batch.Request().PostAsync(batchRequestContent).ConfigureAwait(true);

                User user = await returnedResponse.GetResponseByIdAsync<User>(userRequestId).ConfigureAwait(true);
                Presence presence = await returnedResponse.GetResponseByIdAsync<Presence>(presenceRequestId).ConfigureAwait(true);

                return (User: user, Presence: presence);
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error Occured", e);
                _diagClient.TrackException(e);
                throw;
            }
        }

        #endregion

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            lblHueSaved.Visibility = Visibility.Collapsed;
            lblLIFXSaved.Visibility = Visibility.Collapsed;
            lblSettingSaved.Visibility = Visibility.Collapsed;
        }

        #region Tray Methods

        protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            await _settingsService.SaveSettings(Config).ConfigureAwait(true);
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
            Helpers.AppendLogger(_logger, "Turning On PresenceLight Sync");
        }

        private async void OnTurnOffSyncClick(object sender, RoutedEventArgs e)
        {
            try
            {
                lightMode = "Custom";

                if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedHueLightId))
                {
                    if (Config.LightSettings.Hue.UseRemoteApi)
                    {
                        await _remoteHueService.SetColor("Off", Config.LightSettings.Hue.SelectedHueLightId, Config.LightSettings.Hue.RemoteBridgeId).ConfigureAwait(true);
                    }
                    else
                    {
                        await _hueService.SetColor("Off", Config.LightSettings.Hue.SelectedHueLightId).ConfigureAwait(true);
                    }
                }

                if (Config.LightSettings.LIFX.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey))
                {

                    await _lifxService.SetColor("Off", Config.LightSettings.LIFX.SelectedLIFXItemId).ConfigureAwait(true);
                }

                turnOffButton.Visibility = Visibility.Collapsed;
                turnOnButton.Visibility = Visibility.Visible;

                notificationIcon.Text = PresenceConstants.Inactive;
                notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, IconConstants.Inactive)));

                this.WindowState = this.lastWindowState;
            }
            catch (Exception ex)
            {
                Helpers.AppendLogger(_logger, "Error Occured", ex);
                _diagClient.TrackException(ex);
            }
            Helpers.AppendLogger(_logger, "Turning Off PresenceLight Sync");
        }

        private async void OnExitClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedHueLightId))
                {
                    if (Config.LightSettings.Hue.UseRemoteApi)
                    {
                        await _remoteHueService.SetColor("Off", Config.LightSettings.Hue.SelectedHueLightId, Config.LightSettings.Hue.RemoteBridgeId).ConfigureAwait(true);
                    }
                    else
                    {
                        await _hueService.SetColor("Off", Config.LightSettings.Hue.SelectedHueLightId).ConfigureAwait(true);
                    }
                }

                if (Config.LightSettings.LIFX.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey))
                {

                    await _lifxService.SetColor("Off", Config.LightSettings.LIFX.SelectedLIFXItemId).ConfigureAwait(true);
                }
                await _settingsService.SaveSettings(Config).ConfigureAwait(true);
                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Helpers.AppendLogger(_logger, "Error Occured", ex);
                _diagClient.TrackException(ex);
            }
            Helpers.AppendLogger(_logger, "PresenceLight Exiting");
        }

        private async void Current_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedHueLightId))
                {
                    if (Config.LightSettings.Hue.UseRemoteApi)
                    {
                        await _remoteHueService.SetColor("Off", Config.LightSettings.Hue.SelectedHueLightId, Config.LightSettings.Hue.RemoteBridgeId).ConfigureAwait(true);
                    }
                    else
                    {
                        await _hueService.SetColor("Off", Config.LightSettings.Hue.SelectedHueLightId).ConfigureAwait(true);
                    }
                }

                if (Config.LightSettings.LIFX.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey))
                {

                    await _lifxService.SetColor("Off", Config.LightSettings.LIFX.SelectedLIFXItemId).ConfigureAwait(true);
                }

                if (Config.LightSettings.Custom.IsCustomApiEnabled && !string.IsNullOrEmpty(Config.LightSettings.Custom.CustomApiOffMethod) && !string.IsNullOrEmpty(Config.LightSettings.Custom.CustomApiOffUri))
                {
                    await _customApiService.SetColor("Off", "Off").ConfigureAwait(true);
                }

                await _settingsService.SaveSettings(Config).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Helpers.AppendLogger(_logger, "Error Occured", ex);
                _diagClient.TrackException(ex);
            }

            Helpers.AppendLogger(_logger, "PresenceLight Session Ending");
        }
        #endregion

        private async Task InteractWithLights()
        {
            try
            {
                while (true)
                {
                    await Task.Delay(Convert.ToInt32(Config.LightSettings.PollingInterval * 1000)).ConfigureAwait(true);

                    bool touchLight = false;
                    string newColor = "";

                    if (Config.LightSettings.SyncLights)
                    {
                        if (!Config.LightSettings.UseWorkingHours)
                        {
                            if (lightMode == "Graph")
                            {
                                touchLight = true;
                            }
                        }
                        else
                        {
                            bool previousWorkingHours = IsWorkingHours;
                            if (IsInWorkingHours())
                            {
                                if (lightMode == "Graph")
                                {
                                    touchLight = true;
                                }
                            }
                            else
                            {
                                // check to see if working hours have passed
                                if (previousWorkingHours)
                                {
                                    switch (Config.LightSettings.HoursPassedStatus)
                                    {
                                        case "Keep":
                                            break;
                                        case "White":
                                            newColor = "Offline";
                                            break;
                                        case "Off":
                                            newColor = "Off";
                                            break;
                                        default:
                                            break;
                                    }
                                    touchLight = true;
                                }

                            }
                        }
                    }

                    if (touchLight)
                    {
                        switch (lightMode)
                        {
                            case "Graph":
                                Helpers.AppendLogger(_logger, "PresenceLight Running in Teams Mode");
                                presence = await System.Threading.Tasks.Task.Run(() => GetPresence()).ConfigureAwait(true);

                                if (newColor == string.Empty)
                                {
                                    await SetColor(presence.Availability, presence.Activity).ConfigureAwait(true);
                                }
                                else
                                {
                                    await SetColor(newColor, presence.Activity).ConfigureAwait(true);
                                }


                                if (DateTime.Now.AddMinutes(-5) > settingsLastSaved)
                                {
                                    await _settingsService.SaveSettings(Config).ConfigureAwait(true);
                                    settingsLastSaved = DateTime.Now;
                                }

                                MapUI(presence, null, null);
                                break;
                            case "Theme":
                                Helpers.AppendLogger(_logger, "PresenceLight Running in Theme Mode");
                                try
                                {
                                    var theme = ((SolidColorBrush)SystemParameters.WindowGlassBrush).Color;
                                    var color = $"#{theme.ToString().Substring(3)}";

                                    lblTheme.Content = $"Theme Color is {color}";
                                    lblTheme.Foreground = (SolidColorBrush)SystemParameters.WindowGlassBrush;
                                    lblTheme.Visibility = Visibility.Visible;

                                    if (lightMode == "Theme")
                                    {
                                        await SetColor(color).ConfigureAwait(true);
                                    }

                                    if (DateTime.Now.Minute % 5 == 0)
                                    {
                                        await _settingsService.SaveSettings(Config).ConfigureAwait(true);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Helpers.AppendLogger(_logger, "Error Occured", ex);
                                    _diagClient.TrackException(ex);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error Occured", e);
                _diagClient.TrackException(e);
            }
        }
    }
}

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using LifxCloud.NET.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;

using PresenceLight.Core;
using PresenceLight.Graph;
using PresenceLight.Services;
using PresenceLight.Telemetry;

using Windows.ApplicationModel;

using Media = System.Windows.Media;
using Package = Windows.ApplicationModel.Package;

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

        private MediatR.IMediator _mediator;
        private LIFXOAuthHelper _lIFXOAuthHelper;



        private readonly IGraphService _graphservice;
        private DiagnosticsClient _diagClient;
        private ISettingsService _settingsService;
        private IWorkingHoursService _workingHoursService;
        private WindowState lastWindowState;
        private bool previousRemoteFlag;
        private readonly ILogger<MainWindow> _logger;

        #region Init
        public MainWindow(IGraphService graphService,
                          IWorkingHoursService workingHoursService,
                          MediatR.IMediator mediator,
                          IOptionsMonitor<BaseConfig> optionsAccessor,
                          LIFXOAuthHelper lifxOAuthHelper,
                          DiagnosticsClient diagClient,
                          ILogger<MainWindow> logger,
                          ISettingsService settingsService)
        {
            _logger = logger;
            InitializeComponent();
            System.Windows.Application.Current.SessionEnding += new SessionEndingCancelEventHandler(Current_SessionEnding);

            LoadAboutMe();

            _workingHoursService = workingHoursService;
            _graphservice = graphService;


            logs.LogFilePath = App.StaticConfig["Serilog:WriteTo:1:Args:Path"];

            _mediator = mediator;
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

                var tbContext = landingPage.notificationIcon.DataContext;
                DataContext = Config;
                landingPage.notificationIcon.DataContext = tbContext;

                if (Config.StartMinimized)
                {
                    this.Hide();
                }
            });
        }, TaskScheduler.Current);
        }

        private void LoadAboutMe()
        {
            about.packageName.Text = ThisAppInfo.GetDisplayName();
            about.packageVersion.Text = ThisAppInfo.GetPackageVersion();
            about.installedFrom.Text = ThisAppInfo.GetAppInstallerUri();
            about.installLocation.Text = ThisAppInfo.GetInstallLocation();
            about.settingsLocation.Text = ThisAppInfo.GetSettingsLocation();
            about.installedDate.Text = ThisAppInfo.GetInstallationDate();
            about.RuntimeVersionInfo.Text = ThisAppInfo.GetDotNetRuntimeInfo();

            if (Convert.ToBoolean(App.StaticConfig["IsAppPackaged"], CultureInfo.InvariantCulture))
            {
                about.updateBtn.Visibility = Visibility.Visible;
            }
            else
            {
                about.updateBtn.Visibility = Visibility.Collapsed;
            }
        }

        private void SettingsLinkClick(object sender, RoutedEventArgs e)
        {
            string filePath = ThisAppInfo.GetSettingsLocation();
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogError("Settings File Not Found");
            }
            else
            {
                //Clean up file path so it can be navigated OK
                filePath = System.IO.Path.GetFullPath(filePath);
                System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
            }
        }

        private async void CheckForUpdates(object sender, RoutedEventArgs e)
        {
            var result = await Package.Current.CheckUpdateAvailabilityAsync();
            if (result.Availability == PackageUpdateAvailability.Available)
            {
                MessageBox.Show("There's a new update! Restart your app to install it");
            }
        }

        private void LoadApp()
        {
            try
            {
                CheckHue(true);
                CheckLIFX();
                CheckYeelight();
                CheckWiz();
                CheckAAD();

                previousRemoteFlag = Config.LightSettings.Hue.UseRemoteApi;

                if (Config.IconType == "Transparent")
                {
                    settings.Transparent.IsChecked = true;
                }
                else
                {
                    settings.White.IsChecked = true;
                }

                switch (Config.LightSettings.HoursPassedStatus)
                {
                    case "Keep":
                        settings.HourStatusKeep.IsChecked = true;
                        break;
                    case "White":
                        settings.HourStatusWhite.IsChecked = true;
                        break;
                    case "Off":
                        settings.HourStatusOff.IsChecked = true;
                        break;
                    default:
                        settings.HourStatusKeep.IsChecked = true;
                        break;
                }

                PopulateWorkingDays();

                landingPage.notificationIcon.Text = PresenceConstants.Inactive;
                landingPage.notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(String.Empty, IconConstants.Inactive)));

                landingPage.turnOffButton.Visibility = Visibility.Collapsed;
                landingPage.turnOnButton.Visibility = Visibility.Collapsed;

                Config.LightSettings.WorkingHoursStartTimeAsDate = string.IsNullOrEmpty(Config.LightSettings.WorkingHoursStartTime) ? null : DateTime.Parse(Config.LightSettings.WorkingHoursStartTime, null);
                Config.LightSettings.WorkingHoursEndTimeAsDate = string.IsNullOrEmpty(Config.LightSettings.WorkingHoursEndTime) ? null : DateTime.Parse(Config.LightSettings.WorkingHoursEndTime, null);


                CallGraph().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occured - {e.Message}");
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
            lightColors.syncTeamsButton.IsEnabled = false;
            lightColors.syncThemeButton.IsEnabled = true;

            if (!await _mediator.Send(new Core.GraphServices.GetIsInitializedCommand()))
            {
                await _mediator.Send(new Core.GraphServices.InitializeCommand()
                {
                    Client = _graphservice.GetAuthenticatedGraphClient()
                });

            }


            landingPage.signInPanel.Visibility = Visibility.Collapsed;
            lightColors.lblTheme.Visibility = Visibility.Collapsed;
            landingPage.loadingPanel.Visibility = Visibility.Visible;

            try
            {
                var (profile, presence) = await _mediator.Send(new Core.GraphServices.GetProfileAndPresenceCommand());

                var photo = await GetPhoto().ConfigureAwait(true);

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
                    if (!await _mediator.Send(new Core.WorkingHoursServices.UseWorkingHoursCommand()))
                    {
                        if (lightMode == "Graph")
                        {
                            await SetColor(presence.Availability, presence.Activity).ConfigureAwait(true);
                        }
                    }
                    else
                    {
                        bool previousWorkingHours = await _mediator.Send(new Core.WorkingHoursServices.IsInWorkingHoursCommand());
                        if (previousWorkingHours)
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

                landingPage.loadingPanel.Visibility = Visibility.Collapsed;
                landingPage.signInPanel.Visibility = Visibility.Collapsed;


                landingPage.dataPanel.Visibility = Visibility.Visible;
                await _settingsService.SaveSettings(Config).ConfigureAwait(true);

                landingPage.turnOffButton.Visibility = Visibility.Visible;
                landingPage.turnOnButton.Visibility = Visibility.Collapsed;

                await InteractWithLights().ConfigureAwait(true);
            }

            catch (Exception e)
            {
                _logger.LogError(e, "Error occured");
                _diagClient.TrackException(e);
            }
        }

        public async Task SetColor(string color, string? activity = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedItemId))
                {
                    if (Config.LightSettings.Hue.UseRemoteApi)
                    {
                        if (!string.IsNullOrEmpty(Config.LightSettings.Hue.RemoteBridgeId))
                        {
                            await _mediator.Send(new Core.RemoteHueServices.SetColorCommand
                            {
                                Availability = color,
                                LightId = Config.LightSettings.Hue.SelectedItemId,
                                BridgeId = Config.LightSettings.Hue.RemoteBridgeId
                            }).ConfigureAwait(true);
                        }
                    }
                    else
                    {
                        await _mediator.Send(new Core.HueServices.SetColorCommand()
                        {
                            Activity = activity,
                            Availability = color,
                            LightID = Config.LightSettings.Hue.SelectedItemId
                        }).ConfigureAwait(false);
                    }
                }

                if (Config.LightSettings.LIFX.IsEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey))
                {
                    await _mediator.Send(new PresenceLight.Core.LifxServices.SetColorCommand { Activity = activity, Availability = color, LightId = Config.LightSettings.LIFX.SelectedItemId }).ConfigureAwait(true);

                }

                if (Config.LightSettings.Wiz.IsEnabled)
                {
                    await _mediator.Send(new PresenceLight.Core.WizServices.SetColorCommand { Activity = activity, Availability = color, LightID = Config.LightSettings.Wiz.SelectedItemId }).ConfigureAwait(true);

                }

                if (Config.LightSettings.Yeelight.IsEnabled && !string.IsNullOrEmpty(Config.LightSettings.Yeelight.SelectedItemId))
                {
                    await _mediator.Send(new PresenceLight.Core.YeelightServices.SetColorCommand { Activity = activity, Availability = color, LightId = Config.LightSettings.Yeelight.SelectedItemId }).ConfigureAwait(true);

                }

                if (Config.LightSettings.CustomApi.IsEnabled)
                {
                    string response = await _mediator.Send(new Core.CustomApiServices.SetColorCommand() { Activity = activity, Availability = color });
                    this.Dispatcher.Invoke(() =>
                    {
                        customapi.customApiLastResponse.Content = response;
                        if (response.Contains("Error:", StringComparison.OrdinalIgnoreCase))
                        {
                            customapi.customApiLastResponse.Foreground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            customapi.customApiLastResponse.Foreground = new SolidColorBrush(Colors.Green);
                        }
                    });

                }
            }
            catch (Exception e)
            {

                _logger.LogError(e, "Error Occurred");
                _diagClient.TrackException(e);
                //throw;
            }
        }

        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("Signing out of Graph PresenceLight Sync");

            lightMode = "Graph";
            var accounts = await WPFAuthorizationProvider.Application.GetAccountsAsync().ConfigureAwait(true);
            if (accounts.Any())
            {
                try
                {
                    await WPFAuthorizationProvider.Application.RemoveAsync(accounts.FirstOrDefault()).ConfigureAwait(true);

                    landingPage.signInPanel.Visibility = Visibility.Visible;
                    landingPage.dataPanel.Visibility = Visibility.Collapsed;

                    landingPage.notificationIcon.Text = PresenceConstants.Inactive;
                    landingPage.notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, IconConstants.Inactive)));


                    await SetColor("Off").ConfigureAwait(true);
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
                _logger.LogError(e, "Error Occured in LoadImager");
                _diagClient.TrackException(e);
                throw;
            }
        }

        public Media.Color MapColor(string hexColor)
        {
            return (Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hexColor);
        }

        public void MapUI(Presence presence, User? profile, BitmapImage? profileImageBit)
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
                        landingPage.notificationIcon.Text = PresenceConstants.Available;
                        break;
                    case "Busy":
                        image = new BitmapImage(new Uri(IconConstants.GetIcon(Config.IconType, IconConstants.Busy)));
                        color = MapColor("#ff3300");
                        landingPage.notificationIcon.Text = PresenceConstants.Busy;
                        break;
                    case "BeRightBack":
                        image = new BitmapImage(new Uri(IconConstants.GetIcon(Config.IconType, IconConstants.BeRightBack)));
                        color = MapColor("#ffff00");
                        landingPage.notificationIcon.Text = PresenceConstants.BeRightBack;
                        break;
                    case "Away":
                        image = new BitmapImage(new Uri(IconConstants.GetIcon(Config.IconType, IconConstants.Away)));
                        color = MapColor("#ffff00");
                        landingPage.notificationIcon.Text = PresenceConstants.Away;
                        break;
                    case "DoNotDisturb":
                        image = new BitmapImage(new Uri(IconConstants.GetIcon(Config.IconType, IconConstants.DoNotDisturb)));
                        color = MapColor("#B03CDE");
                        landingPage.notificationIcon.Text = PresenceConstants.DoNotDisturb;
                        break;
                    case "OutOfOffice":
                        image = new BitmapImage(new Uri(IconConstants.GetIcon(Config.IconType, IconConstants.OutOfOffice)));
                        color = MapColor("#800080");
                        landingPage.notificationIcon.Text = PresenceConstants.OutOfOffice;
                        break;
                    default:
                        image = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, IconConstants.Inactive)));
                        color = MapColor("#FFFFFF");
                        landingPage.notificationIcon.Text = PresenceConstants.Inactive;
                        break;
                }

                if (profileImageBit != null)
                {
                    landingPage.profileImage.Source = profileImageBit;
                }

                landingPage.notificationIcon.Icon = image;
                mySolidColorBrush.Color = color;
                landingPage.status.Fill = mySolidColorBrush;
                landingPage.status.StrokeThickness = 1;
                landingPage.status.Stroke = System.Windows.Media.Brushes.Black;

                if (profile != null)
                {
                    landingPage.userName.Content = profile.DisplayName;
                }

                landingPage.activity.Content = "Activity: " + presence.Activity;
                landingPage.availability.Content = "Availability: " + presence.Availability;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Occurred");
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
                return await _mediator.Send(new Core.GraphServices.GetPresenceCommand());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Occurred");
                _diagClient.TrackException(e);
                throw;
            }
        }

        public async Task<byte[]?> GetPhoto()
        {
            try
            {
                var photo = await _mediator.Send(new Core.GraphServices.GetPhotoCommand());

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
                return null;
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

        #endregion

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            yeelight.lblYeelightSaved.Visibility = Visibility.Collapsed;
            philipsHue.lblHueSaved.Visibility = Visibility.Collapsed;
            lifx.lblLIFXSaved.Visibility = Visibility.Collapsed;
            customapi.lblCustomApiSaved.Visibility = Visibility.Collapsed;
            settings.lblSettingSaved.Visibility = Visibility.Collapsed;
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

            landingPage.turnOffButton.Visibility = Visibility.Visible;
            landingPage.turnOnButton.Visibility = Visibility.Collapsed;

            this.WindowState = this.lastWindowState;
            _logger.LogInformation("Turning On PresenceLight Sync");
        }

        private async void OnTurnOffSyncClick(object sender, RoutedEventArgs e)
        {
            try
            {
                lightMode = "Custom";

                await SetColor("Off", "Off");

                landingPage.turnOffButton.Visibility = Visibility.Collapsed;
                landingPage.turnOnButton.Visibility = Visibility.Visible;

                landingPage.notificationIcon.Text = PresenceConstants.Inactive;
                landingPage.notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, IconConstants.Inactive)));

                this.WindowState = this.lastWindowState;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured");
                _diagClient.TrackException(ex);
            }
            _logger.LogInformation("Turning Off PresenceLight Sync");
        }

        private async void OnExitClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await SetColor("Off", "Off");

                await _settingsService.SaveSettings(Config).ConfigureAwait(true);
                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured");
                _diagClient.TrackException(ex);
            }
            _logger.LogInformation("PresenceLight Exiting");
        }

        private async void Current_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            try
            {
                await SetColor("Off", "Off");

                await _settingsService.SaveSettings(Config).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured");
                _diagClient.TrackException(ex);
            }

            _logger.LogInformation("PresenceLight Session Ending");
        }
        #endregion

        private async Task InteractWithLights()
        {
            bool previousWorkingHours = false;
            string previousLightMode = string.Empty;
            while (true)
            {
                try
                {
                    await Task.Delay(Convert.ToInt32(Config.LightSettings.PollingInterval * 1000)).ConfigureAwait(true);

                    bool touchLight = false;
                    string newColor = "";

                    if (Config.LightSettings.SyncLights)
                    {
                        if (!await _mediator.Send(new Core.WorkingHoursServices.UseWorkingHoursCommand()))
                        {
                            if (lightMode == "Graph")
                            {
                                touchLight = true;
                            }
                        }
                        else
                        {
                            var isInWorkingHours = await _mediator.Send(new Core.WorkingHoursServices.IsInWorkingHoursCommand());
                            if (isInWorkingHours)
                            {
                                previousWorkingHours = isInWorkingHours;
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
                                    previousWorkingHours = false;
                                    previousLightMode = lightMode;
                                    switch (Config.LightSettings.HoursPassedStatus)
                                    {

                                        case "White":
                                            newColor = "Offline";
                                            lightMode = "Manual";
                                            break;
                                        case "Off":
                                            newColor = "Off";
                                            lightMode = "Manual";
                                            break;
                                        case "Keep":
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
                            case "Manual":
                                // No need to check presence... if it's after hours, we just want to action upon it... 
                                await SetColor(newColor, presence.Activity).ConfigureAwait(true);
                                //Reset the light mode so that we don't potentially mess something up.
                                lightMode = previousLightMode;
                                break;
                            case "Graph":
                                _logger.LogInformation("PresenceLight Running in Teams Mode");
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
                                _logger.LogInformation("PresenceLight Running in Theme Mode");
                                try
                                {
                                    var theme = ((SolidColorBrush)SystemParameters.WindowGlassBrush).Color;
                                    var color = $"#{theme.ToString().Substring(3)}";

                                    lightColors.lblTheme.Content = $"Theme Color is {color}";
                                    lightColors.lblTheme.Foreground = (SolidColorBrush)SystemParameters.WindowGlassBrush;
                                    lightColors.lblTheme.Visibility = Visibility.Visible;

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
                                    _logger.LogError(ex, "Error Occured");
                                    _diagClient.TrackException(ex);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error Occurred");
                    _diagClient.TrackException(e);
                }
            }
        }
    }
}

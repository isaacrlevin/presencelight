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
            CheckHueSettings();
            CheckLIFXSettings();
            CheckYeelightSettings();
            CheckAAD();

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
            if (Config.UseWorkingHours)
            {
                pnlWorkingHours.Visibility = Visibility.Visible;
                SyncOptions();
            }
            else
            {
                pnlWorkingHours.Visibility = Visibility.Collapsed;
                SyncOptions();
            }

            if (Config.IsPhillipsEnabled)
            {
                pnlPhillips.Visibility = Visibility.Visible;
                SyncOptions();
            }
            else
            {
                pnlPhillips.Visibility = Visibility.Collapsed;
            }

            if (Config.IsYeelightEnabled)
            {
                pnlYeelight.Visibility = Visibility.Visible;
                SyncOptions();
            }
            else
            {
                pnlYeelight.Visibility = Visibility.Collapsed;
            }

            if (Config.IsLIFXEnabled)
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

            if (Config.IsCustomApiEnabled)
            {
                pnlCustomApi.Visibility = Visibility.Visible;
                customApiAvailableMethod.SelectedValue = Config.CustomApiAvailableMethod;
                customApiBusyMethod.SelectedValue = Config.CustomApiBusyMethod;
                customApiBeRightBackMethod.SelectedValue = Config.CustomApiBeRightBackMethod;
                customApiAwayMethod.SelectedValue = Config.CustomApiAwayMethod;
                customApiDoNotDisturbMethod.SelectedValue = Config.CustomApiDoNotDisturbMethod;
                customApiOfflineMethod.SelectedValue = Config.CustomApiOfflineMethod;
                customApiOffMethod.SelectedValue = Config.CustomApiOffMethod;
                SyncOptions();
            }
            else
            {
                pnlCustomApi.Visibility = Visibility.Collapsed;
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
            string availability = string.Empty;
            while (true)
            {
                await Task.Delay(Convert.ToInt32(Config.PollingInterval * 1000));
                if (Config.UseWorkingHours)
                {
                    if (IsInWorkingHours(Config.WorkingHoursStartTime, Config.WorkingHoursEndTime))
                    {
                        try
                        {
                            presence = await System.Threading.Tasks.Task.Run(() => GetPresence());

                            if (lightMode == "Graph")
                            {
                                if (presence.Availability != availability)
                                {
                                    await SetColor(presence.Availability);
                                }
                            }

                            if (DateTime.Now.Minute % 5 == 0)
                            {
                                await SettingsService.SaveSettings(Config);
                            }

                            MapUI(presence, null, null);
                            availability = presence.Availability;
                        }
                        catch { }
                    }
                }
            }
        }

        public async Task SetColor(string color)
        {
            if (!string.IsNullOrEmpty(Config.HueApiKey) && !string.IsNullOrEmpty(Config.HueIpAddress) && !string.IsNullOrEmpty(Config.SelectedHueLightId))
            {
                await _hueService.SetColor(color, Config.SelectedHueLightId);
            }

            if (Config.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LIFXApiKey))
            {
                await _lifxService.SetColor(color, (Selector)Config.SelectedLIFXItemId);
            }

            if (Config.IsYeelightEnabled && !string.IsNullOrEmpty(Config.SelectedYeeLightId))
            {
                await _yeelightService.SetColor(color, Config.SelectedYeeLightId);
            }

            if (Config.IsCustomApiEnabled)
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

                    if (Config.IsPhillipsEnabled && !string.IsNullOrEmpty(Config.HueApiKey) && !string.IsNullOrEmpty(Config.HueIpAddress) && !string.IsNullOrEmpty(Config.SelectedHueLightId))
                    {
                        await _hueService.SetColor("Off", Config.SelectedHueLightId);
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
            Config.Brightness = Convert.ToInt32(brightness.Value);

            SyncOptions();
            await SettingsService.SaveSettings(Config);
            lblSettingSaved.Visibility = Visibility.Visible;
            btnSettings.IsEnabled = true;
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

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            lblHueSaved.Visibility = Visibility.Collapsed;
            lblLIFXSaved.Visibility = Visibility.Collapsed;
            lblSettingSaved.Visibility = Visibility.Collapsed;
        }

        private async void cbSyncTeamsPresence(object sender, RoutedEventArgs e)
        {
            if (Config.SyncTeamsPresence)
            {
                lightMode = "Graph";
                syncTeamsButton.IsEnabled = false;
                syncThemeButton.IsEnabled = true;
            }
            else
            {
                lightMode = "Custom";
                syncTeamsButton.IsEnabled = true;
                syncThemeButton.IsEnabled = true;
            }
            SyncOptions();
            await SettingsService.SaveSettings(Config);
            e.Handled = true;
        }

        private void cbUseWorkingHoursChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Config.WorkingHoursStartTime))
            {
                Config.WorkingHoursStartTime = DateTime.Parse(Config.WorkingHoursStartTime).TimeOfDay.ToString();
            }

            if (!string.IsNullOrEmpty(Config.WorkingHoursEndTime))
            {
                Config.WorkingHoursEndTime = DateTime.Parse(Config.WorkingHoursEndTime).TimeOfDay.ToString();
            }

            if (Config.UseWorkingHours)
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

            if (Config.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LIFXApiKey))
            {

                await _lifxService.SetColor("Off", (Selector)Config.SelectedLIFXItemId);
            }
            await SettingsService.SaveSettings(Config);
            System.Windows.Application.Current.Shutdown();
        }
        private async void Current_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            if (!string.IsNullOrEmpty(Config.HueApiKey) && !string.IsNullOrEmpty(Config.HueIpAddress) && !string.IsNullOrEmpty(Config.SelectedHueLightId))
            {
                await _hueService.SetColor("Off", Config.SelectedHueLightId);
            }

            if (Config.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LIFXApiKey))
            {

                await _lifxService.SetColor("Off", (Selector)Config.SelectedLIFXItemId);
            }
            await SettingsService.SaveSettings(Config);
        }
        #endregion

        bool IsInWorkingHours(string startString, string endString)
        {
            // convert datetime to a TimeSpan
            TimeSpan start = TimeSpan.Parse(startString);
            TimeSpan end = TimeSpan.Parse(endString);
            TimeSpan now = DateTime.Now.TimeOfDay;
            // see if start comes before end
            if (start < end)
                return start <= now && now <= end;
            // start is after end, so do the inverse comparison
            return !(end < now && now < start);
        }

        private void time_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!string.IsNullOrEmpty(Config.WorkingHoursStartTime))
            {
                Config.WorkingHoursStartTime = DateTime.Parse(Config.WorkingHoursStartTime).TimeOfDay.ToString();
            }

            if (!string.IsNullOrEmpty(Config.WorkingHoursEndTime))
            {
                Config.WorkingHoursEndTime = DateTime.Parse(Config.WorkingHoursEndTime).TimeOfDay.ToString();
            }
        }
    }
}

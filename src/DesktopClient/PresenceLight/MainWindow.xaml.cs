using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;

using PresenceLight.Core;
using PresenceLight.Graph;
using PresenceLight.Telemetry;

namespace PresenceLight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BaseConfig _options;


        // private Presence presence { get; set; }
        private DateTime settingsLastSaved = DateTime.MinValue;

        private MediatR.IMediator _mediator;

        private readonly IGraphService _graphservice;
        private DiagnosticsClient _diagClient;
        private ISettingsService _settingsService;
        private WindowState lastWindowState;
        private bool isInteractRunning;
        private readonly ILogger<MainWindow> _logger;
        private readonly AppState _appState;

        #region Init
        public MainWindow(IGraphService graphService,
                          MediatR.IMediator mediator,
                          IOptionsMonitor<BaseConfig> optionsAccessor,
                          DiagnosticsClient diagClient,
                          ILogger<MainWindow> logger,
                          ISettingsService settingsService,
                          AppState appState)
        {
            var currentApp = (App)System.Windows.Application.Current;
            Resources.Add("services", currentApp.ServiceProvider);
            InitializeComponent();
            _appState = appState;

            _logger = logger;
            System.Windows.Application.Current.SessionEnding += new SessionEndingCancelEventHandler(Current_SessionEnding);

            _graphservice = graphService;


            _mediator = mediator;
            _options = optionsAccessor != null ? optionsAccessor.CurrentValue : throw new NullReferenceException("Options Accessor is null");
            _diagClient = diagClient;
            _settingsService = settingsService;

            LoadSettings().ContinueWith(
        async t =>
        {
            if (t.IsFaulted)
            { }

            await Task.Run(async () =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    appState.SignedIn = false;
                    LoadApp();

                    var tbContext = notificationIcon.DataContext;
                    DataContext = _appState.Config;
                    notificationIcon.DataContext = tbContext;

                    if (_appState.Config.StartMinimized)
                    {
                        this.Hide();
                    }

                });

                while (true)
                {

                    await Task.Run(async () =>
                    {
                        Thread.Sleep(100);
                        if (_appState.SignInRequested)
                        {
                            _appState.SignInRequested = false;

                            await this.Dispatcher.Invoke(async () =>
                             {
                                 await SignIn();
                             });
                        }

                        if (_appState.SignOutRequested)
                        {
                            _appState.SignOutRequested = false;
                            await this.Dispatcher.Invoke(async () =>
                             {
                                 await SignOut();
                             });
                        }
                    });
                }
            });


        }, TaskScheduler.Current);
        }

        private async Task LoadSettings()
        {
            try
            {
                _logger.LogInformation("Load Settings Initialized");
                if (!(await _settingsService.IsFilePresent().ConfigureAwait(true)))
                {
                    await _settingsService.SaveSettings(_options).ConfigureAwait(true);
                }

                _appState.SetConfig(await _settingsService.LoadSettings().ConfigureAwait(true) ?? throw new NullReferenceException("Settings Load Service Returned null"));

                bool useWorkingHours = await _mediator.Send(new Core.WorkingHoursServices.UseWorkingHoursCommand());
                bool IsInWorkingHours = await _mediator.Send(new Core.WorkingHoursServices.IsInWorkingHoursCommand());
                _logger.LogInformation("Load Settings Successfull");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occured Loading Settings");
                _diagClient.TrackException(e);
            }
        }
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await blazorWebView1.WebView.EnsureCoreWebView2Async();
            blazorWebView1.WebView.CoreWebView2.Settings.IsZoomControlEnabled = false;
        }
        private void LoadApp()
        {
            try
            {
                notificationIcon.Text = $"PresenceLight Status - {PresenceConstants.Inactive}";
                notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, string.Empty)));

                _appState.Config.LightSettings.WorkingHoursStartTimeAsDate = string.IsNullOrEmpty(_appState.Config.LightSettings.WorkingHoursStartTime) ? null : DateTime.Parse(_appState.Config.LightSettings.WorkingHoursStartTime, null);
                _appState.Config.LightSettings.WorkingHoursEndTimeAsDate = string.IsNullOrEmpty(_appState.Config.LightSettings.WorkingHoursEndTime) ? null : DateTime.Parse(_appState.Config.LightSettings.WorkingHoursEndTime, null);

                CallGraph().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occured - {e.Message}");
            }
        }

        #endregion

        #region Profile Panel

        private async Task SignIn()
        {
            await CallGraph().ConfigureAwait(true);
        }

        private async Task CallGraph()
        {
            _appState.SetLightMode("Graph");
            _logger.LogInformation("Light Mode Set: Graph");
            if (!await _mediator.Send(new Core.GraphServices.GetIsInitializedCommand()))
            {
                await _mediator.Send(new Core.GraphServices.InitializeCommand()
                {
                    Client = _graphservice.GetAuthenticatedGraphClient()
                });

            }

            _appState.SignedIn = true;

            try
            {
                await _settingsService.SaveSettings(_appState.Config).ConfigureAwait(true);

                if (!isInteractRunning)
                {
                    await InteractWithLights().ConfigureAwait(true);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occured calling Graph");
            }
        }

        public async Task SetColor(string color, string? activity = null)
        {
            try
            {
                if (_appState.Config.LightSettings.Hue.IsEnabled)
                {
                    if (!string.IsNullOrEmpty(_appState.Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(_appState.Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(_appState.Config.LightSettings.Hue.SelectedItemId))
                    {
                        if (_appState.Config.LightSettings.Hue.UseRemoteApi)
                        {
                            if (!string.IsNullOrEmpty(_appState.Config.LightSettings.Hue.RemoteBridgeId))
                            {
                                await _mediator.Send(new Core.RemoteHueServices.SetColorCommand
                                {
                                    Availability = color,
                                    LightId = _appState.Config.LightSettings.Hue.SelectedItemId,
                                    BridgeId = _appState.Config.LightSettings.Hue.RemoteBridgeId
                                }).ConfigureAwait(true);
                            }
                        }
                        else
                        {
                            await _mediator.Send(new Core.HueServices.SetColorCommand()
                            {
                                Activity = activity,
                                Availability = color,
                                LightID = _appState.Config.LightSettings.Hue.SelectedItemId
                            }).ConfigureAwait(false);
                        }
                    }
                }

                if (_appState.Config.LightSettings.LIFX.IsEnabled && !string.IsNullOrEmpty(_appState.Config.LightSettings.LIFX.LIFXApiKey))
                {
                    await _mediator.Send(new PresenceLight.Core.LifxServices.SetColorCommand { Activity = activity, Availability = color, LightId = _appState.Config.LightSettings.LIFX.SelectedItemId }).ConfigureAwait(true);

                }

                if (_appState.Config.LightSettings.Wiz.IsEnabled)
                {
                   // await _mediator.Send(new PresenceLight.Core.WizServices.SetColorCommand { Activity = activity, Availability = color, LightID = _appState.Config.LightSettings.Wiz.SelectedItemId }).ConfigureAwait(true);

                }

                if (_appState.Config.LightSettings.Yeelight.IsEnabled && !string.IsNullOrEmpty(_appState.Config.LightSettings.Yeelight.SelectedItemId))
                {
                    await _mediator.Send(new PresenceLight.Core.YeelightServices.SetColorCommand { Activity = activity, Availability = color, LightId = _appState.Config.LightSettings.Yeelight.SelectedItemId }).ConfigureAwait(true);

                }

                if (_appState.Config.LightSettings.CustomApi.IsEnabled)
                {
                    string response = await _mediator.Send(new Core.CustomApiServices.SetColorCommand() { Activity = activity, Availability = color });
                }

                if(_appState.Config.LightSettings.LocalSerialHost.IsEnabled)
                {
                    string response = await _mediator.Send(new Core.LocalSerialHostServices.SetColorCommand() { Activity = activity, Availability = color });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occured Setting Color");
            }
        }

        private async Task SignOut()
        {
            _logger.LogInformation("Signing out of Graph PresenceLight Sync");

            _appState.SetLightMode("Graph");
            var accounts = await WPFAuthorizationProvider.Application.GetAccountsAsync().ConfigureAwait(true);
            if (accounts.Any())
            {
                try
                {
                    await WPFAuthorizationProvider.Application.RemoveAsync(accounts.FirstOrDefault()).ConfigureAwait(true);
                    _appState.SignedIn = false;
                    _appState.SetUserInfo(null, null, null);
                    notificationIcon.Text = $"PresenceLight Status - {PresenceConstants.Inactive}";
                    notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, string.Empty)));

                    await SetColor("Off").ConfigureAwait(true);
                }
                catch (MsalException)
                {
                }
            }
            await _settingsService.SaveSettings(_appState.Config).ConfigureAwait(true);
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
                throw;
            }
        }

        public void MapUI(Presence presence)
        {
            try
            {
                SolidColorBrush mySolidColorBrush = new SolidColorBrush();
                if (presence != null)
                {
                    notificationIcon.Text = $"PresenceLight Status - {Helpers.HumanifyText(presence.Availability)}";
                    notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(_appState.Config.IconType, presence.Availability)));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Occurred Mapping UI");
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
                _logger.LogError(e, "Error occured Getting Presence");
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
                _logger.LogError(e, "Error occured Getting Photo");
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

        #endregion

        #region Tray Methods

        protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            await _settingsService.SaveSettings(_appState.Config).ConfigureAwait(true);
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
            _appState.SetLightMode("Graph");

            this.WindowState = this.lastWindowState;
            _logger.LogInformation("Turning On PresenceLight Sync");
        }

        private async void OnTurnOffSyncClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _appState.SetLightMode("Custom");
                await SetColor("Off", "Off");

                notificationIcon.Text = PresenceConstants.Inactive;
                notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, string.Empty)));

                this.WindowState = this.lastWindowState;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured turning Off Sync");
            }
            _logger.LogInformation("Turning Off PresenceLight Sync");
        }

        private async void OnExitClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await SetColor("Off", "Off");

                await _settingsService.SaveSettings(_appState.Config).ConfigureAwait(true);
                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured Exiting");
            }
            _logger.LogInformation("PresenceLight Exiting");
        }

        private async void Current_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            try
            {
                await SetColor("Off", "Off");

                await _settingsService.SaveSettings(_appState.Config).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured Ending Session");
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
                isInteractRunning = true;
                try
                {
                    if (_appState.SignedIn)
                    {
                        if (_appState.User == null)
                        {
                            var (profile, presence) = await _mediator.Send(new Core.GraphServices.GetProfileAndPresenceCommand());

                            var photo = await GetPhoto().ConfigureAwait(true);

                            _appState.SetLightMode("Graph");

                            if (photo == null)
                            {
                                MapUI(presence);
                                _appState.SetUserInfo(profile, presence);
                            }
                            else
                            {
                                MapUI(presence);
                                _appState.SetUserInfo(profile, presence, $"data:image/gif;base64,{Convert.ToBase64String(photo)}");
                            }
                        }
                        await Task.Delay(Convert.ToInt32(_appState.Config.LightSettings.PollingInterval * 1000)).ConfigureAwait(true);

                        bool touchLight = false;
                        string newColor = "";

                        if (_appState.Config.LightSettings.SyncLights)
                        {
                            if (!await _mediator.Send(new Core.WorkingHoursServices.UseWorkingHoursCommand()))
                            {
                                if (_appState.LightMode == "Graph")
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
                                    if (_appState.LightMode == "Graph")
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
                                        previousLightMode = _appState.LightMode;
                                        switch (_appState.Config.LightSettings.HoursPassedStatus)
                                        {

                                            case "White":
                                                newColor = "Offline";
                                                _appState.SetLightMode("Manual");
                                                break;
                                            case "Off":
                                                newColor = "Off";
                                                _appState.SetLightMode("Manual");
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

                        if (touchLight && _appState.SignedIn)
                        {
                            switch (_appState.LightMode)
                            {
                                case "Manual":
                                    // No need to check presence... if it's after hours, we just want to action upon it... 
                                    await SetColor(newColor, _appState.Presence.Activity).ConfigureAwait(true);
                                    //Reset the light mode so that we don't potentially mess something up.
                                    _appState.SetLightMode(previousLightMode);
                                    break;
                                case "Graph":
                                    _logger.LogInformation("PresenceLight Running in Teams Mode");

                                    _appState.SetPresence(await System.Threading.Tasks.Task.Run(() => GetPresence()).ConfigureAwait(true));

                                    if (newColor == string.Empty)
                                    {
                                        await SetColor(_appState.Presence.Availability, _appState.Presence.Activity).ConfigureAwait(true);
                                    }
                                    else
                                    {
                                        await SetColor(newColor, _appState.Presence.Activity).ConfigureAwait(true);
                                    }
                                    if (DateTime.Now.AddMinutes(-5) > settingsLastSaved)
                                    {
                                        await _settingsService.SaveSettings(_appState.Config).ConfigureAwait(true);
                                        settingsLastSaved = DateTime.Now;
                                    }

                                    MapUI(_appState.Presence);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        isInteractRunning = false;
                        break;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error occured interacting with lights");
                }
            }
        }
    }
}

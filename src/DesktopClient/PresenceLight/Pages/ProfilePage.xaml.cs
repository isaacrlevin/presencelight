using System;
using System.Collections.Generic;
using System.IO;
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;

using PresenceLight.Core;
using PresenceLight.Core.GraphServices;
using PresenceLight.Graph;
using PresenceLight.Services;
using PresenceLight.Telemetry;

namespace PresenceLight.Pages
{
    public partial class ProfilePage
    {
        private MainWindowModern _parentWindow;

        private DateTime settingsLastSaved = DateTime.MinValue;
        public static string LightMode { get; set; }
        public ProfilePage()
        {
            _parentWindow = System.Windows.Application.Current.Windows.OfType<MainWindowModern>().First();
            InitializeComponent();

            _parentWindow.notificationIcon.Text = PresenceConstants.Inactive;
            _parentWindow.notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(String.Empty, IconConstants.Inactive)));

            if (_parentWindow.presence != null && _parentWindow.profile != null)
            {
                if (_parentWindow.photo == null)
                {
                    MapUI(_parentWindow.presence, _parentWindow.profile, new BitmapImage(new Uri("pack://application:,,,/PresenceLight;component/images/UnknownProfile.png")));
                }
                else
                {
                    MapUI(_parentWindow.presence, _parentWindow.profile, _parentWindow.photo.LoadImage());
                }

                loadingPanel.Visibility = Visibility.Collapsed;
                signInPanel.Visibility = Visibility.Collapsed;


                dataPanel.Visibility = Visibility.Visible;


                _parentWindow.turnOffButton.Visibility = Visibility.Visible;
                _parentWindow.turnOnButton.Visibility = Visibility.Collapsed;
            }
        }

        protected override void Start()
        {
            CheckAAD();
            CallGraph().ConfigureAwait(true);
            base.Start();
        }
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

        public async Task CallGraph()
        {
            LightMode = "Graph";
            if (!await _parentWindow._mediator.Send(new Core.GraphServices.GetIsInitializedCommand()))
            {
                await _parentWindow._mediator.Send(new Core.GraphServices.InitializeCommand()
                {
                    Client = _parentWindow._graphservice.GetAuthenticatedGraphClient()
                });
            }

            signInPanel.Visibility = Visibility.Collapsed;
            loadingPanel.Visibility = Visibility.Visible;

            try
            {
                (_parentWindow.profile, _parentWindow.presence) = await _parentWindow._mediator.Send(new Core.GraphServices.GetProfileAndPresenceCommand());
                _parentWindow.photo = await _parentWindow._mediator.Send(new GetPhotoCommand()).ConfigureAwait(true);


                if (_parentWindow.photo == null)
                {
                    MapUI(_parentWindow.presence, _parentWindow.profile, new BitmapImage(new Uri("pack://application:,,,/PresenceLight;component/images/UnknownProfile.png")));
                }
                else
                {
                    MapUI(_parentWindow.presence, _parentWindow.profile, _parentWindow.photo.LoadImage());
                }


                if (SettingsHandlerBase.Config.LightSettings.SyncLights)
                {
                    if (!await _parentWindow._mediator.Send(new Core.WorkingHoursServices.UseWorkingHoursCommand()))
                    {
                        if (LightMode == "Graph")
                        {
                            await _parentWindow._mediator.Send(new SetColorCommand { Color = "Off" }).ConfigureAwait(true);
                        }
                    }
                    else
                    {
                        bool previousWorkingHours = await _parentWindow._mediator.Send(new Core.WorkingHoursServices.IsInWorkingHoursCommand());
                        if (previousWorkingHours)
                        {
                            if (LightMode == "Graph")
                            {
                                await _parentWindow._mediator.Send(new Services.SetColorCommand { Activity = _parentWindow.presence.Activity, Color = _parentWindow.presence.Availability }).ConfigureAwait(true);
                            }
                        }
                        else
                        {
                            // check to see if working hours have passed
                            if (previousWorkingHours)
                            {
                                if (LightMode == "Graph")
                                {
                                    switch (SettingsHandlerBase.Config.LightSettings.HoursPassedStatus)
                                    {

                                        case "White":
                                            await _parentWindow._mediator.Send(new SetColorCommand { Activity = _parentWindow.presence.Activity, Color = "Offline" }).ConfigureAwait(true);
                                            break;
                                        case "Off":
                                            await _parentWindow._mediator.Send(new SetColorCommand { Activity = _parentWindow.presence.Activity, Color = "Off" }).ConfigureAwait(true);
                                            break;
                                        default:
                                            await _parentWindow._mediator.Send(new SetColorCommand { Activity = _parentWindow.presence.Activity, Color = _parentWindow.presence.Availability }).ConfigureAwait(true);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }

                loadingPanel.Visibility = Visibility.Collapsed;
                signInPanel.Visibility = Visibility.Collapsed;


                dataPanel.Visibility = Visibility.Visible;
                await _parentWindow._mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);


                _parentWindow.turnOffButton.Visibility = Visibility.Visible;
                _parentWindow.turnOnButton.Visibility = Visibility.Collapsed;

                await InteractWithLights().ConfigureAwait(true);
            }

            catch (Exception e)
            {
                _parentWindow._logger.LogError(e, "Error occured");
                _parentWindow._diagClient.TrackException(e);
            }
        }

        private async Task InteractWithLights()
        {
            bool previousWorkingHours = false;
            string previousLightMode = string.Empty;
            while (true)
            {
                try
                {
                    await Task.Delay(Convert.ToInt32(SettingsHandlerBase.Config.LightSettings.PollingInterval * 1000)).ConfigureAwait(true);

                    bool touchLight = false;
                    string newColor = "";

                    if (SettingsHandlerBase.Config.LightSettings.SyncLights)
                    {
                        if (!await _parentWindow._mediator.Send(new Core.WorkingHoursServices.UseWorkingHoursCommand()))
                        {
                            if (LightMode == "Graph")
                            {
                                touchLight = true;
                            }
                        }
                        else
                        {
                            var isInWorkingHours = await _parentWindow._mediator.Send(new Core.WorkingHoursServices.IsInWorkingHoursCommand());
                            if (isInWorkingHours)
                            {
                                previousWorkingHours = isInWorkingHours;
                                if (LightMode == "Graph")
                                {
                                    touchLight = true;
                                }
                            }
                            else
                            {
                                // check to see if working hours have passed
                                if (previousWorkingHours)
                                {
                                    previousLightMode = LightMode;
                                    switch (SettingsHandlerBase.Config.LightSettings.HoursPassedStatus)
                                    {
                                        case "White":
                                            LightMode = "Manual";
                                            newColor = "Offline";
                                            break;
                                        case "Off":
                                            LightMode = "Manual";
                                            newColor = "Off";
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
                        switch (LightMode)
                        {
                            case "Manual":
                                // No need to check presence... if it's after hours, we just want to action upon it... 
                                await _parentWindow._mediator.Send(new SetColorCommand { Activity = _parentWindow.presence.Activity, Color = newColor }).ConfigureAwait(true);
                                //Reset the light mode so that we don't potentially mess something up.
                                LightMode = previousLightMode;
                                break;
                            case "Graph":
                                _parentWindow._logger.LogInformation("PresenceLight Running in Teams Mode");
                                _parentWindow.presence = await _parentWindow._mediator.Send(new Core.GraphServices.GetPresenceCommand());

                                if (newColor == string.Empty)
                                {
                                    await _parentWindow._mediator.Send(new SetColorCommand { Activity = _parentWindow.presence.Activity, Color = _parentWindow.presence.Availability }).ConfigureAwait(true);
                                }
                                else
                                {
                                    await _parentWindow._mediator.Send(new SetColorCommand { Activity = _parentWindow.presence.Activity, Color = newColor }).ConfigureAwait(true);
                                }

                                if (DateTime.Now.AddMinutes(-5) > settingsLastSaved)
                                {
                                    await _parentWindow._mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);

                                    settingsLastSaved = DateTime.Now;
                                }

                                MapUI(_parentWindow.presence, null, null);
                                break;
                            case "Theme":
                                _parentWindow._logger.LogInformation("PresenceLight Running in Theme Mode");
                                try
                                {
                                    var theme = ((SolidColorBrush)SystemParameters.WindowGlassBrush).Color;
                                    var color = $"#{theme.ToString().Substring(3)}";
                                    var lightColors = System.Windows.Application.Current.Windows.OfType<Pages.CustomColorPage>().First();



                                    lightColors.lblTheme.Content = $"Theme Color is {color}";
                                    lightColors.lblTheme.Foreground = (SolidColorBrush)SystemParameters.WindowGlassBrush;
                                    lightColors.lblTheme.Visibility = Visibility.Visible;

                                    if (LightMode == "Theme")
                                    {
                                        await _parentWindow._mediator.Send(new SetColorCommand { Color = color }).ConfigureAwait(true);
                                    }

                                    if (DateTime.Now.Minute % 5 == 0)
                                    {
                                        await _parentWindow._mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);


                                    }
                                }
                                catch (Exception ex)
                                {
                                    _parentWindow._logger.LogError(ex, "Error Occured");
                                    _parentWindow._diagClient.TrackException(ex);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    _parentWindow._logger.LogError(e, "Error Occurred");
                    _parentWindow._diagClient.TrackException(e);
                }
            }
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
                        image = new BitmapImage(new Uri(IconConstants.GetIcon(SettingsHandlerBase.Config.IconType, IconConstants.Available)));
                        color = "#009933".MapColor();
                        _parentWindow.notificationIcon.Text = PresenceConstants.Available;
                        break;
                    case "Busy":
                        image = new BitmapImage(new Uri(IconConstants.GetIcon(SettingsHandlerBase.Config.IconType, IconConstants.Busy)));
                        color = "#ff3300".MapColor();
                        _parentWindow.notificationIcon.Text = PresenceConstants.Busy;
                        break;
                    case "BeRightBack":
                        image = new BitmapImage(new Uri(IconConstants.GetIcon(SettingsHandlerBase.Config.IconType, IconConstants.BeRightBack)));
                        color = "#ffff00".MapColor();
                        _parentWindow.notificationIcon.Text = PresenceConstants.BeRightBack;
                        break;
                    case "Away":
                        image = new BitmapImage(new Uri(IconConstants.GetIcon(SettingsHandlerBase.Config.IconType, IconConstants.Away)));
                        color = "#ffff00".MapColor();
                        _parentWindow.notificationIcon.Text = PresenceConstants.Away;
                        break;
                    case "DoNotDisturb":
                        image = new BitmapImage(new Uri(IconConstants.GetIcon(SettingsHandlerBase.Config.IconType, IconConstants.DoNotDisturb)));
                        color = "#B03CDE".MapColor();
                        _parentWindow.notificationIcon.Text = PresenceConstants.DoNotDisturb;
                        break;
                    case "OutOfOffice":
                        image = new BitmapImage(new Uri(IconConstants.GetIcon(SettingsHandlerBase.Config.IconType, IconConstants.OutOfOffice)));
                        color = "#800080".MapColor();
                        _parentWindow.notificationIcon.Text = PresenceConstants.OutOfOffice;
                        break;
                    default:
                        image = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, IconConstants.Inactive)));
                        color = "#FFFFFF".MapColor();
                        _parentWindow.notificationIcon.Text = PresenceConstants.Inactive;
                        break;
                }

                if (profileImageBit != null)
                {
                    profileImage.Source = profileImageBit;
                }

                _parentWindow.notificationIcon.Icon = image;
                mySolidColorBrush.Color = color;
                status.Fill = mySolidColorBrush;
                status.StrokeThickness = 1;
                status.Stroke = System.Windows.Media.Brushes.Black;

                if (profile != null)
                {
                    userName.Content = profile.DisplayName;
                }

                activity.Content = "Activity: " + Helpers.HumanifyText(presence.Activity);
                availability.Content = "Availability: " + Helpers.HumanifyText(presence.Availability);
            }
            catch (Exception e)
            {
                _parentWindow._logger.LogError(e, "Error Occurred");
                _parentWindow._diagClient.TrackException(e);
                throw;
            }
        }

        public async void CheckAAD()
        {
            try
            {
                SettingsHandlerBase.SyncOptions();

                configErrorPanel.Visibility = Visibility.Hidden;

                if (dataPanel.Visibility != Visibility.Visible)
                {
                    signInPanel.Visibility = Visibility.Visible;
                }

                if (!await _parentWindow._mediator.Send(new Core.GraphServices.GetIsInitializedCommand()))
                {
                    await _parentWindow._mediator.Send(new Core.GraphServices.InitializeCommand()
                    {
                        Client = _parentWindow._graphservice.GetAuthenticatedGraphClient()
                    });

                }
            }
            catch (Exception e)
            {
                _parentWindow._logger.LogError(e, "Error occured Checking Azure Active Directory");
                _parentWindow._diagClient.TrackException(e);
            }
        }

        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            _parentWindow._logger.LogInformation("Signing out of Graph PresenceLight Sync");

            LightMode = "Graph";
            var accounts = await WPFAuthorizationProvider.Application.GetAccountsAsync().ConfigureAwait(true);
            if (accounts.Any())
            {
                try
                {
                    await WPFAuthorizationProvider.Application.RemoveAsync(accounts.FirstOrDefault()).ConfigureAwait(true);

                    signInPanel.Visibility = Visibility.Visible;
                    dataPanel.Visibility = Visibility.Collapsed;

                    _parentWindow.notificationIcon.Text = PresenceConstants.Inactive;
                    _parentWindow.notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, IconConstants.Inactive)));

                    if (SettingsHandlerBase.Config.LightSettings.Hue.IsEnabled && !string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.Hue.SelectedItemId))
                    {
                        if (SettingsHandlerBase.Config.LightSettings.Hue.UseRemoteApi)
                        {
                            await _parentWindow._mediator.Send(new Core.RemoteHueServices.SetColorCommand
                            {
                                Availability = "Off",
                                LightId = SettingsHandlerBase.Config.LightSettings.Hue.SelectedItemId,
                                BridgeId = SettingsHandlerBase.Config.LightSettings.Hue.RemoteBridgeId
                            }).ConfigureAwait(true);
                        }
                        else
                        {
                            await _parentWindow._mediator.Send(new Core.HueServices.SetColorCommand() { Availability = "Off", LightID = SettingsHandlerBase.Config.LightSettings.Hue.SelectedItemId, Activity = "" }).ConfigureAwait(true);

                        }
                    }

                    if (LightMode == "Graph")
                    {
                        await _parentWindow._mediator.Send(new SetColorCommand { Color = "Off" }).ConfigureAwait(true);


                    }
                }
                catch (MsalException)
                {
                }
            }
            _parentWindow.presence = null;
            _parentWindow.profile = null;
            _parentWindow.profile = null;
            await _parentWindow._mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);

        }
        #endregion

    }
}

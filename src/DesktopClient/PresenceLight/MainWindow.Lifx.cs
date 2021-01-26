using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using LifxCloud.NET.Models;
using PresenceLight.Telemetry;
using System.Windows.Navigation;
using PresenceLight.Core;

namespace PresenceLight
{
    public partial class MainWindow : Window
    {
        #region LIFX Panel

        private async void LIFXToken_Get(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                string accessToken = await _lIFXOAuthHelper.InitiateTokenRetrieval().ConfigureAwait(true);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    lifxApiKey.Text = accessToken;
                    Config.LightSettings.LIFX.LIFXApiKey = accessToken;
                    SyncOptions();
                }
                this.Activate();
            }
            catch (Exception ex)
            {
                Helpers.AppendLogger(_logger, "Error occured Getting LIFX Token", ex);
                _diagClient.TrackException(ex);
            }
        }

        private async void SaveLIFX_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnLIFX.IsEnabled = false;
                Config = Helpers.CleanColors(Config);
                await _settingsService.SaveSettings(Config).ConfigureAwait(true);
                lblLIFXSaved.Visibility = Visibility.Visible;
                btnLIFX.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Helpers.AppendLogger(_logger, "Error occured Saving LIFX Settings", ex);
                _diagClient.TrackException(ex);
            }
        }

        private async void CheckLIFX()
        {
            imgLIFXLoading.Visibility = Visibility.Visible;
            pnlLIFXBrightness.Visibility = Visibility.Collapsed;
            lblLIFXMessage.Visibility = Visibility.Collapsed;

            SolidColorBrush fontBrush = new SolidColorBrush();
            try
            {
                if (Config.LightSettings.LIFX.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey) && !string.IsNullOrEmpty(Config.LightSettings.LIFX.SelectedLIFXItemId))
                {
                    ddlLIFXLights.ItemsSource = await _lifxService.GetAllLights().ConfigureAwait(true);

                    foreach (var item in ddlLIFXLights.Items)
                    {
                        if (item != null)
                        {
                            var light = (Light)item;
                            if ($"id:{light?.Id}" == Config.LightSettings.LIFX.SelectedLIFXItemId)
                            {
                                ddlLIFXLights.SelectedItem = item;
                            }
                        }
                    }

                    if (ddlLIFXLights.SelectedItem == null)
                    {
                        ddlLIFXLights.ItemsSource = await _lifxService.GetAllGroups().ConfigureAwait(true);

                        foreach (var item in ddlLIFXLights.Items)
                        {
                            if (item != null)
                            {
                                var group = (LifxCloud.NET.Models.Group)item;
                                if ($"group_id:{group?.Id}" == Config.LightSettings.LIFX.SelectedLIFXItemId)
                                {
                                    ddlLIFXLights.SelectedItem = item;
                                }
                            }
                        }
                    }

                    if (ddlLIFXLights.SelectedItem != null)
                    {

                        pnlLIFXBrightness.Visibility = Visibility.Visible;
                        lblLIFXMessage.Text = "Connected to LIFX Cloud";
                        fontBrush.Color = MapColor("#009933");
                        lblLIFXMessage.Foreground = fontBrush;
                    }
                }
            }
            catch (Exception ex)
            {
                _diagClient.TrackException(ex);
                Helpers.AppendLogger(_logger, "Error occured Checking LIFX", ex);
                lblLIFXMessage.Text = "Error Occured Connecting to LIFX, please try again";
                fontBrush.Color = MapColor("#ff3300");
                lblLIFXMessage.Foreground = fontBrush;
            }

            imgLIFXLoading.Visibility = Visibility.Collapsed;
        }

        private void ddlLIFXLights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddlLIFXLights.SelectedItem != null)
            {
                // Get whether item is group or light
                if (ddlLIFXLights.SelectedItem.GetType() == typeof(LifxCloud.NET.Models.Group))
                {
                    Config.LightSettings.LIFX.SelectedLIFXItemId = $"group_id:{((LifxCloud.NET.Models.Group)ddlLIFXLights.SelectedItem).Id}";
                }

                if (ddlLIFXLights.SelectedItem.GetType() == typeof(LifxCloud.NET.Models.Light))
                {
                    Config.LightSettings.LIFX.SelectedLIFXItemId = $"id:{((LifxCloud.NET.Models.Light)ddlLIFXLights.SelectedItem).Id}";
                }

                SyncOptions();
            }
            e.Handled = true;
        }

        private async void CheckLIFX_Click(object sender, RoutedEventArgs e)
        {
            imgLIFXLoading.Visibility = Visibility.Visible;
            pnlLIFXBrightness.Visibility = Visibility.Collapsed;
            lblLIFXMessage.Visibility = Visibility.Collapsed;
            SolidColorBrush fontBrush = new SolidColorBrush();

            if (!string.IsNullOrEmpty(lifxApiKey.Text))
            {
                try
                {
                    Config.LightSettings.LIFX.LIFXApiKey = lifxApiKey.Text;

                    SyncOptions();
                    if (((System.Windows.Controls.Button)sender).Name == "btnGetLIFXGroups")
                    {
                        ddlLIFXLights.ItemsSource = await _lifxService.GetAllGroups().ConfigureAwait(true);
                    }
                    else
                    {
                        ddlLIFXLights.ItemsSource = await _lifxService.GetAllLights().ConfigureAwait(true);
                    }
                    lblLIFXMessage.Visibility = Visibility.Visible;
                    pnlLIFXBrightness.Visibility = Visibility.Visible;
                    lblLIFXMessage.Text = "Connected to LIFX Cloud";
                    fontBrush.Color = MapColor("#009933");
                    lblLIFXMessage.Foreground = fontBrush;
                }
                catch (Exception ex)
                {
                    Helpers.AppendLogger(_logger, "Error Getting LIFX Lights", ex);
                    _diagClient.TrackException(ex);
                    lblLIFXMessage.Visibility = Visibility.Visible;
                    pnlLIFXBrightness.Visibility = Visibility.Collapsed;
                    lblLIFXMessage.Text = "Error Occured Connecting to LIFX, please try again";
                    fontBrush.Color = MapColor("#ff3300");
                    lblLIFXMessage.Foreground = fontBrush;
                }
            }
            else
            {
                Run run1 = new Run("Valid LIFX Key Required ");
                Run run2 = new Run(" https://cloud.lifx.com/settings");

                Hyperlink hyperlink = new Hyperlink(run2)
                {
                    NavigateUri = new Uri("https://cloud.lifx.com/settings")
                };
                hyperlink.RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler(Hyperlink_RequestNavigate); //to be implemented
                lblLIFXMessage.Inlines.Clear();
                lblLIFXMessage.Inlines.Add(run1);
                lblLIFXMessage.Inlines.Add(hyperlink);


                fontBrush.Color = MapColor("#ff3300");
                lblLIFXMessage.Foreground = fontBrush;

            }

            imgLIFXLoading.Visibility = Visibility.Collapsed;
        }

        private void cbIsLIFXEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.LIFX.IsLIFXEnabled)
            {
                getTokenLink.Visibility = Visibility.Visible;
                pnlLIFX.Visibility = Visibility.Visible;
            }
            else
            {
                getTokenLink.Visibility = Visibility.Collapsed;
                pnlLIFX.Visibility = Visibility.Collapsed;
            }

            SyncOptions();
            e.Handled = true;
        }

        private void cbIsLIFXAvailableStatusDisabledChanged(object sender, RoutedEventArgs e)
        {
            lifxAvailableColour.IsEnabled = !Config.LightSettings.LIFX.AvailableStatus.Disabled;
            SyncOptions();
            e.Handled = true;
        }

        private void cbIsLIFXBusyStatusDisabledChanged(object sender, RoutedEventArgs e)
        {
            lifxBusyColour.IsEnabled = !Config.LightSettings.LIFX.BusyStatus.Disabled;
            SyncOptions();
            e.Handled = true;
        }

        private void cbIsLIFXAwayStatusDisabledChanged(object sender, RoutedEventArgs e)
        {
            lifxAwayColour.IsEnabled = !Config.LightSettings.LIFX.AwayStatus.Disabled;
            SyncOptions();
            e.Handled = true;
        }

        private void cbIsLIFXDoNotDisturbStatusDisabledChanged(object sender, RoutedEventArgs e)
        {
            lifxDoNotDisturbColour.IsEnabled = !Config.LightSettings.LIFX.DoNotDisturbStatus.Disabled;
            SyncOptions();
            e.Handled = true;
        }

        private void cbIsLIFXBeRightBackStatusDisabledChanged(object sender, RoutedEventArgs e)
        {
            lifxBeRightBackColour.IsEnabled = !Config.LightSettings.LIFX.BeRightBackStatus.Disabled;
            SyncOptions();
            e.Handled = true;
        }

        private void cbIsLIFXOfflineStatusDisabledChanged(object sender, RoutedEventArgs e)
        {
            lifxOfflineColour.IsEnabled = !Config.LightSettings.LIFX.OfflineStatus.Disabled;
            SyncOptions();
            e.Handled = true;
        }

        private void cbIsLIFXOffStatusDisabledChanged(object sender, RoutedEventArgs e)
        {
            lifxOffColour.IsEnabled = !Config.LightSettings.LIFX.OffStatus.Disabled;
            SyncOptions();
            e.Handled = true;
        }

        #endregion
    }
}

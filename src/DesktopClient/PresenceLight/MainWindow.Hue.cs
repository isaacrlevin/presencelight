using System;
using PresenceLight.Core;
using System.Windows;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using PresenceLight.Telemetry;
using ABI.Windows.System.RemoteSystems;
using System.Windows.Navigation;

namespace PresenceLight
{
    public partial class MainWindow : Window
    {
        #region Hue Panel

        private async void SaveHue_Click(object sender, RoutedEventArgs e)
        {
            btnHue.IsEnabled = false;
            Config = Helpers.CleanColors(Config);
            await _settingsService.SaveSettings(Config).ConfigureAwait(true);

            if (Config.LightSettings.Hue.UseRemoteApi)
            {
                _remoteHueService = new RemoteHueService(Config);
            }
            else
            {
                _hueService = new HueService(Config);
            }
            CheckHue();
            lblHueSaved.Visibility = Visibility.Visible;
            btnHue.IsEnabled = true;
        }

        private async void HueApiKey_Get(object sender, RequestNavigateEventArgs e)
        {
            var (bridgeId, apiKey, bridgeIp) = await _remoteHueService.RegisterBridge();
            if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(bridgeId) && !string.IsNullOrEmpty(bridgeIp))
            {
                Config.LightSettings.Hue.HueApiKey = apiKey;
                Config.LightSettings.Hue.RemoteBridgeId = bridgeId;
                Config.LightSettings.Hue.HueIpAddress = bridgeIp;

                await _settingsService.SaveSettings(Config);

                ddlHueLights.ItemsSource = await _remoteHueService.CheckLights();
                SyncOptions();

                SolidColorBrush fontBrush = new SolidColorBrush();
                pnlHueBrightness.Visibility = Visibility.Visible;
                lblHueMessage.Text = "App Registered with Bridge";
                fontBrush.Color = MapColor("#009933");
                lblHueMessage.Foreground = fontBrush;

                pnlHueBrightness.Visibility = Visibility.Visible;
                imgHueLoading.Visibility = Visibility.Collapsed;
                lblHueMessage.Visibility = Visibility.Visible;
            }
            this.Activate();
        }

        private void ddlHueLights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddlHueLights.SelectedItem != null)
            {
                Config.LightSettings.Hue.SelectedHueLightId = ((Q42.HueApi.Light)ddlHueLights.SelectedItem).Id;
                SyncOptions();
            }
            e.Handled = true;
        }

        private void HueIpAddress_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (((TextBox)e.OriginalSource).Text.Trim() != ((TextBox)e.Source).Text.Trim())
            {
                if (Config != null)
                {
                    Config.LightSettings.Hue.HueApiKey = String.Empty;
                }
                SyncOptions();
            }
            CheckHue();
            e.Handled = true;
        }
        private async void CheckHue()
        {
            try
            {
                if (Config != null)
                {
                    SolidColorBrush fontBrush = new SolidColorBrush();

                    if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress))
                    {
                        hueIpAddress.Text = Config.LightSettings.Hue.HueIpAddress;
                    }

                    if ((!Config.LightSettings.Hue.UseRemoteApi || !string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey)) && !CheckHueIp())
                    {
                        lblHueMessage.Text = "Valid IP Address Required";
                        fontBrush.Color = MapColor("#ff3300");
                        btnRegister.IsEnabled = false;
                        pnlHueBrightness.Visibility = Visibility.Collapsed;
                        lblHueMessage.Foreground = fontBrush;
                    }
                    else
                    {
                        Config.LightSettings.Hue.HueIpAddress = hueIpAddress.Text;

                        SyncOptions();

                        btnRegister.IsEnabled = true;
                        if (string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey))
                        {
                            lblHueMessage.Text = "Missing App Registration, please press button on bridge then click 'Register Bridge'";
                            fontBrush.Color = MapColor("#ff3300");
                            pnlHueBrightness.Visibility = Visibility.Collapsed;
                            lblHueMessage.Foreground = fontBrush;
                        }
                        else
                        {
                            if (Config.LightSettings.Hue.UseRemoteApi)
                            {
                                ddlHueLights.ItemsSource = await _remoteHueService.CheckLights();
                            }
                            else
                            {
                                ddlHueLights.ItemsSource = await _hueService.CheckLights();
                            }

                            foreach (var item in ddlHueLights.Items)
                            {
                                var light = (Q42.HueApi.Light)item;
                                if (light?.Id == Config.LightSettings.Hue.SelectedHueLightId)
                                {
                                    ddlHueLights.SelectedItem = item;
                                }
                            }
                            pnlHueBrightness.Visibility = Visibility.Visible;
                            lblHueMessage.Text = "App Registered with Bridge";
                            fontBrush.Color = MapColor("#009933");
                            lblHueMessage.Foreground = fontBrush;
                        }
                    }
                }
            }
            catch { }
        }

        private bool CheckHueIp()
        {
            string r2 = @"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b";

            Regex r = new Regex(r2);

            if (string.IsNullOrEmpty(hueIpAddress.Text.Trim()) || !r.IsMatch(hueIpAddress.Text.Trim()) || hueIpAddress.Text.Trim().EndsWith(".", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }

        private async void FindBridge_Click(object sender, RoutedEventArgs e)
        {
            Config.LightSettings.Hue.HueIpAddress = await _hueService.FindBridge().ConfigureAwait(true);
        }

        private void cbIsPhillipsEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.Hue.IsPhillipsHueEnabled)
            {
                pnlPhillips.Visibility = Visibility.Visible;
                pnlHueApi.Visibility = Visibility.Visible;
            }
            else
            {
                pnlPhillips.Visibility = Visibility.Collapsed;
                pnlHueApi.Visibility = Visibility.Collapsed;
            }
            SyncOptions();
            e.Handled = true;
        }

        private void cbUseRemoteApiChanged(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.Hue.UseRemoteApi)
            {
                hueIpAddress.IsEnabled = false;
                btnFindBridge.IsEnabled = false;
            }
            else
            {
                hueIpAddress.IsEnabled = true;
                btnFindBridge.IsEnabled = true;
            }
            SyncOptions();
            e.Handled = true;
        }

        private void cbIsPhillipsAvailableStatusDisabledChanged(object sender, RoutedEventArgs e)
        {
            hueAvailableColour.IsEnabled = !Config.LightSettings.Hue.AvailableStatus.Disabled;
            SyncOptions();
            e.Handled = true;
        }

        private void cbIsPhillipsBusyStatusDisabledChanged(object sender, RoutedEventArgs e)
        {
            hueBusyColour.IsEnabled = !Config.LightSettings.Hue.BusyStatus.Disabled;
            SyncOptions();
            e.Handled = true;
        }

        private void cbIsPhillipsAwayStatusDisabledChanged(object sender, RoutedEventArgs e)
        {
            hueAwayColour.IsEnabled = !Config.LightSettings.Hue.AwayStatus.Disabled;
            SyncOptions();
            e.Handled = true;
        }

        private void cbIsPhillipsDoNotDisturbStatusDisabledChanged(object sender, RoutedEventArgs e)
        {
            hueDoNotDisturbColour.IsEnabled = !Config.LightSettings.Hue.DoNotDisturbStatus.Disabled;
            SyncOptions();
            e.Handled = true;
        }

        private void cbIsPhillipsBeRightBackStatusDisabledChanged(object sender, RoutedEventArgs e)
        {
            hueBeRightBackColour.IsEnabled = !Config.LightSettings.Hue.BeRightBackStatus.Disabled;
            SyncOptions();
            e.Handled = true;
        }

        private void cbIsPhillipsOfflineStatusDisabledChanged(object sender, RoutedEventArgs e)
        {
            hueOfflineColour.IsEnabled = !Config.LightSettings.Hue.OfflineStatus.Disabled;
            SyncOptions();
            e.Handled = true;
        }

        private void cbIsPhillipsOffStatusDisabledChanged(object sender, RoutedEventArgs e)
        {
            hueOffColour.IsEnabled = !Config.LightSettings.Hue.OffStatus.Disabled;
            SyncOptions();
            e.Handled = true;
        }

        private async void RegisterBridge_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
            MessageBox.Show("Please press the sync button on your Phillips Hue Bridge");

            SolidColorBrush fontBrush = new SolidColorBrush();

            try
            {
                imgHueLoading.Visibility = Visibility.Visible;
                lblHueMessage.Visibility = Visibility.Collapsed;
                pnlHueBrightness.Visibility = Visibility.Collapsed;
                Config.LightSettings.Hue.HueApiKey = await _hueService.RegisterBridge().ConfigureAwait(true);
                ddlHueLights.ItemsSource = await _hueService.CheckLights().ConfigureAwait(true);
                SyncOptions();
                pnlHueBrightness.Visibility = Visibility.Visible;
                imgHueLoading.Visibility = Visibility.Collapsed;
                lblHueMessage.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                _diagClient.TrackException(ex);

                lblHueMessage.Text = "Error Occured registering bridge, please try again";
                fontBrush.Color = MapColor("#ff3300");
                lblHueMessage.Foreground = fontBrush;
            }

            if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey))
            {
                lblHueMessage.Text = "App Registered with Bridge";
                fontBrush.Color = MapColor("#009933");
                lblHueMessage.Foreground = fontBrush;
            }

            CheckHue();
        }

        #endregion
    }
}

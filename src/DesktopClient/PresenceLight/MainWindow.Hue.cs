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

        private void cbIsPhillipsEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.Hue.IsEnabled)
            {
                pnlPhillips.Visibility = Visibility.Visible;
                pnlHueApi.Visibility = Visibility.Visible;
            }
            else
            {
                pnlPhillips.Visibility = Visibility.Collapsed;
                pnlHueApi.Visibility = Visibility.Collapsed;
            }

            if (Config.LightSettings.Hue.UseRemoteApi)
            {
                hueIpAddress.IsEnabled = false;
                btnFindBridge.IsEnabled = false;
                btnRegister.IsEnabled = false;
                remoteHueButton.IsEnabled = true;
            }
            else
            {
                remoteHueButton.IsEnabled = false;
                hueIpAddress.IsEnabled = true;
                btnFindBridge.IsEnabled = true;
                btnRegister.IsEnabled = true;
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
                btnRegister.IsEnabled = false;
                remoteHueButton.IsEnabled = true;
            }
            else
            {
                remoteHueButton.IsEnabled = false;
                hueIpAddress.IsEnabled = true;
                btnFindBridge.IsEnabled = true;
                btnRegister.IsEnabled = true;
            }

            if (previousRemoteFlag != Config.LightSettings.Hue.UseRemoteApi)
            {
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                MessageBox.Show("You toggled Use Remote Api, if this was intentional, please save.");
            }
            previousRemoteFlag = Config.LightSettings.Hue.UseRemoteApi;
            SyncOptions();
            e.Handled = true;
        }

        private void cbUseHueActivityStatus(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.Hue.UseActivityStatus)
            {
                pnlHueAvailableStatuses.Visibility = Visibility.Collapsed;
                pnlHueActivityStatuses.Visibility = Visibility.Visible;
            }
            else
            {
                pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                pnlHueActivityStatuses.Visibility = Visibility.Collapsed;
            }
            SyncOptions();
            e.Handled = true;
        }

        private void cbHueIsDisabledChange(object sender, RoutedEventArgs e)
        {
            CheckBox cb = e.Source as CheckBox ?? throw new ArgumentException("Check Box Not Found");
            var cbName = cb.Name.Replace("Disabled", "Colour");
            var colorpicker = (Xceed.Wpf.Toolkit.ColorPicker)this.FindName(cbName);
            colorpicker.IsEnabled = !cb.IsChecked.Value;
            SyncOptions();
            e.Handled = true;
        }

        private async void HueApiKey_Get(object sender, RoutedEventArgs e)
        {
            try
            {
                var (bridgeId, apiKey, bridgeIp) = await _remoteHueService.RegisterBridge();
                if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(bridgeId) && !string.IsNullOrEmpty(bridgeIp))
                {
                    hueIpAddress.Text = bridgeIp;
                    Config.LightSettings.Hue.HueApiKey = apiKey;
                    Config.LightSettings.Hue.RemoteBridgeId = bridgeId;
                    Config.LightSettings.Hue.HueIpAddress = bridgeIp;

                    await _settingsService.SaveSettings(Config);

                    ddlHueLights.ItemsSource = await _remoteHueService.GetLights();
                    SyncOptions();

                    SolidColorBrush fontBrush = new SolidColorBrush();
                    pnlHueData.Visibility = Visibility.Visible;
                    lblHueMessage.Text = "App Registered with Bridge";
                    fontBrush.Color = MapColor("#009933");
                    lblHueMessage.Foreground = fontBrush;

                    imgHueLoading.Visibility = Visibility.Collapsed;
                    lblHueMessage.Visibility = Visibility.Visible;

                    if (Config.LightSettings.Hue.UseActivityStatus)
                    {
                        pnlHueActivityStatuses.Visibility = Visibility.Visible;
                        pnlHueAvailableStatuses.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                        pnlHueActivityStatuses.Visibility = Visibility.Collapsed;
                    }

                }
                this.Activate();
            }
            catch (Exception ex)
            {
                Helpers.AppendLogger(_logger, "Error Occured Getting Hue Api Key", ex);
                _diagClient.TrackException(ex);
            }
        }

        private async void FindBridge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Config.LightSettings.Hue.HueIpAddress = await _hueService.FindBridge().ConfigureAwait(true);
                hueIpAddress.Text = Config.LightSettings.Hue.HueIpAddress;

                if (!string.IsNullOrEmpty(hueIpAddress.Text))
                {
                    btnRegister.IsEnabled = true;
                }
                else
                {
                    btnRegister.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Helpers.AppendLogger(_logger, "Error Occured Finding Hue Bridge", ex);
                _diagClient.TrackException(ex);
            }
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
            CheckHue(false);
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
                pnlHueData.Visibility = Visibility.Collapsed;
                Config.LightSettings.Hue.HueApiKey = await _hueService.RegisterBridge().ConfigureAwait(true);
                ddlHueLights.ItemsSource = await _hueService.GetLights().ConfigureAwait(true);
                SyncOptions();
                pnlHueData.Visibility = Visibility.Visible;
                imgHueLoading.Visibility = Visibility.Collapsed;
                lblHueMessage.Visibility = Visibility.Visible;

                if (Config.LightSettings.Hue.UseActivityStatus)
                { }
                else
                {
                    pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                _diagClient.TrackException(ex);
                Helpers.AppendLogger(_logger, "Error Occurred Registering Hue Bridge", ex);
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

            CheckHue(true);
        }

        private void ddlHueLights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddlHueLights.SelectedItem != null)
            {
                Config.LightSettings.Hue.SelectedItemId = ((Q42.HueApi.Light)ddlHueLights.SelectedItem).Id;
                SyncOptions();
            }
            e.Handled = true;
        }

        private async void SaveHue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnHue.IsEnabled = false;
                Config = Helpers.CleanColors(Config);
                await _settingsService.SaveSettings(Config).ConfigureAwait(true);

                if (Config.LightSettings.Hue.UseRemoteApi && _remoteHueService == null)
                {
                    _remoteHueService.Initialize(Config);
                }
                else
                {
                    _hueService.Initialize(Config);
                }
                CheckHue(false);
                lblHueSaved.Visibility = Visibility.Visible;
                btnHue.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Helpers.AppendLogger(_logger, "Error Occured Saving Hue Settings", ex);
                _diagClient.TrackException(ex);
            }
        }

        private async void CheckHue(bool getLights)
        {
            try
            {
                if (Config != null)
                {
                    SolidColorBrush fontBrush = new SolidColorBrush();

                    if (Config.LightSettings.Hue.UseActivityStatus)
                    {
                        pnlHueAvailableStatuses.Visibility = Visibility.Collapsed;
                        pnlHueActivityStatuses.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                        pnlHueActivityStatuses.Visibility = Visibility.Collapsed;
                    }

                    if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress))
                    {
                        hueIpAddress.Text = Config.LightSettings.Hue.HueIpAddress;
                    }

                    if (string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey))
                    {
                        lblHueMessage.Text = "Missing App Registration, please Login to Hue Cloud or Find Local Bridge";
                        fontBrush.Color = MapColor("#ff3300");
                        pnlHueData.Visibility = Visibility.Collapsed;
                        lblHueMessage.Foreground = fontBrush;
                        btnRegister.IsEnabled = false;
                        return;
                    }

                    if (Config.LightSettings.Hue.UseRemoteApi && string.IsNullOrEmpty(Config.LightSettings.Hue.RemoteBridgeId))
                    {
                        lblHueMessage.Text = "Bridge Has Not Been Registered, please Login to Hue Cloud";
                        fontBrush.Color = MapColor("#ff3300");
                        pnlHueData.Visibility = Visibility.Collapsed;
                        lblHueMessage.Foreground = fontBrush;
                        btnRegister.IsEnabled = false;
                        return;
                    }

                    if (!IsValidHueIP())
                    {
                        lblHueMessage.Text = $"IP Address for Bridge Not Valid, please Login to Hue Cloud or Find Local Bridge";
                        fontBrush.Color = MapColor("#ff3300");
                        pnlHueData.Visibility = Visibility.Collapsed;
                        btnRegister.IsEnabled = false;
                        lblHueMessage.Foreground = fontBrush;
                        return;
                    }

                    SyncOptions();

                    if (getLights)
                    {
                        if (Config.LightSettings.Hue.UseRemoteApi)
                        {
                            await _remoteHueService.RegisterBridge();
                            ddlHueLights.ItemsSource = await _remoteHueService.GetLights();
                        }
                        else
                        {
                            ddlHueLights.ItemsSource = await _hueService.GetLights();
                        }

                        foreach (var item in ddlHueLights.Items)
                        {
                            var light = (Q42.HueApi.Light)item;
                            if (light?.Id == Config.LightSettings.Hue.SelectedItemId)
                            {
                                ddlHueLights.SelectedItem = item;
                            }
                        }

                        string registrationMethod;
                        if (Config.LightSettings.Hue.UseRemoteApi)
                        {

                            registrationMethod = "Hue Cloud";
                        }
                        else
                        {
                            registrationMethod = "Local Bridge";
                        }

                        pnlHueData.Visibility = Visibility.Visible;
                        lblHueMessage.Text = $"App Registered with {registrationMethod}";
                        fontBrush.Color = MapColor("#009933");
                        lblHueMessage.Foreground = fontBrush;

                        btnRegister.IsEnabled = true;

                        if (Config.LightSettings.Hue.UseActivityStatus)
                        {
                            pnlHueAvailableStatuses.Visibility = Visibility.Collapsed;
                            pnlHueActivityStatuses.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                            pnlHueActivityStatuses.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.AppendLogger(_logger, "Error Occured Checking Hue Lights", ex);
                _diagClient.TrackException(ex);
            }
        }

        private bool IsValidHueIP()
        {
            string r2 = @"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b";

            Regex r = new Regex(r2);

            if (string.IsNullOrEmpty(hueIpAddress.Text.Trim()) || !r.IsMatch(hueIpAddress.Text.Trim()) || hueIpAddress.Text.Trim().EndsWith(".", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}

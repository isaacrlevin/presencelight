using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using PresenceLight.Core;

using Q42.HueApi;

namespace PresenceLight
{
    public partial class MainWindow : Window
    {
        #region Hue Panel

        private void cbIsPhillipsEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.Hue.IsEnabled)
            {
                phillipsHue.pnlPhillips.Visibility = Visibility.Visible;
                phillipsHue.pnlHueApi.Visibility = Visibility.Visible;
            }
            else
            {
                phillipsHue.pnlPhillips.Visibility = Visibility.Collapsed;
                phillipsHue.pnlHueApi.Visibility = Visibility.Collapsed;
            }

            if (Config.LightSettings.Hue.UseRemoteApi)
            {
                phillipsHue.hueIpAddress.IsEnabled = false;
                phillipsHue.btnFindBridge.IsEnabled = false;
                phillipsHue.btnRegister.IsEnabled = false;
                phillipsHue.remoteHueButton.IsEnabled = true;
            }
            else
            {
                phillipsHue.remoteHueButton.IsEnabled = false;
                phillipsHue.hueIpAddress.IsEnabled = true;
                phillipsHue.btnFindBridge.IsEnabled = true;
                phillipsHue.btnRegister.IsEnabled = true;
            }

            SyncOptions();
            e.Handled = true;
        }

        private void cbUseRemoteApiChanged(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.Hue.UseRemoteApi)
            {
                phillipsHue.hueIpAddress.IsEnabled = false;
                phillipsHue.btnFindBridge.IsEnabled = false;
                phillipsHue.btnRegister.IsEnabled = false;
                phillipsHue.remoteHueButton.IsEnabled = true;
            }
            else
            {
                phillipsHue.remoteHueButton.IsEnabled = false;
                phillipsHue.hueIpAddress.IsEnabled = true;
                phillipsHue.btnFindBridge.IsEnabled = true;
                phillipsHue.btnRegister.IsEnabled = true;
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
                phillipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Collapsed;
                phillipsHue.pnlHueActivityStatuses.Visibility = Visibility.Visible;
            }
            else
            {
                phillipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                phillipsHue.pnlHueActivityStatuses.Visibility = Visibility.Collapsed;
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
                var (bridgeId, apiKey, bridgeIp) = await _mediator.Send(new Core.RemoteHueServices.RegisterBridgeCommand());
                if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(bridgeId) && !string.IsNullOrEmpty(bridgeIp))
                {
                    phillipsHue.hueIpAddress.Text = bridgeIp;
                    Config.LightSettings.Hue.HueApiKey = apiKey;
                    Config.LightSettings.Hue.RemoteBridgeId = bridgeId;
                    Config.LightSettings.Hue.HueIpAddress = bridgeIp;

                    await _settingsService.SaveSettings(Config);

                    phillipsHue.ddlHueLights.ItemsSource = await _mediator.Send(new Core.RemoteHueServices.GetLightsCommand());
                    SyncOptions();

                    SolidColorBrush fontBrush = new SolidColorBrush();
                    phillipsHue.pnlHueData.Visibility = Visibility.Visible;
                    phillipsHue.lblHueMessage.Text = "App Registered with Bridge";
                    fontBrush.Color = MapColor("#009933");
                    phillipsHue.lblHueMessage.Foreground = fontBrush;

                    phillipsHue.imgHueLoading.Visibility = Visibility.Collapsed;
                    phillipsHue.lblHueMessage.Visibility = Visibility.Visible;

                    if (Config.LightSettings.Hue.UseActivityStatus)
                    {
                        phillipsHue.pnlHueActivityStatuses.Visibility = Visibility.Visible;
                        phillipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        phillipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                        phillipsHue.pnlHueActivityStatuses.Visibility = Visibility.Collapsed;
                    }

                }
                this.Activate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured Getting Hue Api Key");
                _diagClient.TrackException(ex);
            }
        }

        private async void FindBridge_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                Config.LightSettings.Hue.HueIpAddress = await _mediator.Send(new Core.HueServices.FindBridgeCommand()).ConfigureAwait(true);

                phillipsHue.hueIpAddress.Text = Config.LightSettings.Hue.HueIpAddress;

                if (!string.IsNullOrEmpty(phillipsHue.hueIpAddress.Text))
                {
                    phillipsHue.btnRegister.IsEnabled = true;
                }
                else
                {
                    phillipsHue.btnRegister.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured Finding Hue Bridge");
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
                phillipsHue.imgHueLoading.Visibility = Visibility.Visible;
                phillipsHue.lblHueMessage.Visibility = Visibility.Collapsed;
                phillipsHue.pnlHueData.Visibility = Visibility.Collapsed;
                Config.LightSettings.Hue.HueApiKey = await _mediator.Send(new Core.HueServices.RegisterBridgeCommand()).ConfigureAwait(true);
                phillipsHue.ddlHueLights.ItemsSource = await _mediator.Send(new Core.HueServices.GetLightsCommand()).ConfigureAwait(true);
                SyncOptions();
                phillipsHue.pnlHueData.Visibility = Visibility.Visible;
                phillipsHue.imgHueLoading.Visibility = Visibility.Collapsed;
                phillipsHue.lblHueMessage.Visibility = Visibility.Visible;

                if (Config.LightSettings.Hue.UseActivityStatus)
                { }
                else
                {
                    phillipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                _diagClient.TrackException(ex);
                _logger.LogError(ex, "Error Occurred Registering Hue Bridge");
                phillipsHue.lblHueMessage.Text = "Error Occured registering bridge, please try again";
                fontBrush.Color = MapColor("#ff3300");
                phillipsHue.lblHueMessage.Foreground = fontBrush;
            }

            if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey))
            {
                phillipsHue.lblHueMessage.Text = "App Registered with Bridge";
                fontBrush.Color = MapColor("#009933");
                phillipsHue.lblHueMessage.Foreground = fontBrush;
            }

            CheckHue(true);
        }

        private void ddlHueLights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (phillipsHue.ddlHueLights.SelectedItem != null)
            {
                // Get whether item is group or light
                if (phillipsHue.ddlHueLights.SelectedItem.GetType() == typeof(Q42.HueApi.Models.Groups.Group))
                {
                    Config.LightSettings.Hue.SelectedItemId = $"group_id:{((Q42.HueApi.Models.Groups.Group)phillipsHue.ddlHueLights.SelectedItem).Id}";
                    phillipsHue.hueItemType.Content = "Groups";
                }

                if (phillipsHue.ddlHueLights.SelectedItem.GetType() == typeof(Light))
                {
                    Config.LightSettings.Hue.SelectedItemId = $"id:{((Light)phillipsHue.ddlHueLights.SelectedItem).Id}";
                    phillipsHue.hueItemType.Content = "Lights";
                }

                SyncOptions();
            }
            e.Handled = true;
        }

        private async void SaveHue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                phillipsHue.btnHue.IsEnabled = false;
                Config = Helpers.CleanColors(Config);
                await _settingsService.SaveSettings(Config).ConfigureAwait(true);

                if (Config.LightSettings.Hue.UseRemoteApi)
                {
                    await _mediator.Send(new Core.RemoteHueServices.InitializeCommand { Options = Config });
                }
                else
                {
                    await _mediator.Send(new Core.HueServices.InitializeCommand { Request = Config });
                }

                CheckHue(false);
                phillipsHue.lblHueSaved.Visibility = Visibility.Visible;
                phillipsHue.btnHue.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured Saving Hue Settings");
                _diagClient.TrackException(ex);
            }
        }

        private async void CheckHue_Click(object sender, RoutedEventArgs e)
        {
            phillipsHue.imgHueLoading.Visibility = Visibility.Visible;
            phillipsHue.pnlHueData.Visibility = Visibility.Collapsed;
            phillipsHue.lblHueMessage.Visibility = Visibility.Collapsed;
            SolidColorBrush fontBrush = new SolidColorBrush();

            if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey))
            {
                try
                {
                    SyncOptions();
                    if (((System.Windows.Controls.Button)e.Source).Name == "btnGetHueGroups")
                    {
                        phillipsHue.ddlHueLights.ItemsSource = await _mediator.Send(new Core.HueServices.GetGroupsCommand());
                        phillipsHue.hueItemType.Content = "Groups";
                    }
                    else
                    {
                        phillipsHue.ddlHueLights.ItemsSource = await _mediator.Send(new Core.HueServices.GetLightsCommand());
                        phillipsHue.hueItemType.Content = "Lights";

                    }

                    phillipsHue.lblHueMessage.Visibility = Visibility.Visible;
                    phillipsHue.pnlHueData.Visibility = Visibility.Visible;
                    phillipsHue.lblHueMessage.Text = "Connected to Hue";

                    phillipsHue.btnGetHueLights.IsEnabled = true;
                    phillipsHue.btnGetHueGroups.IsEnabled = true;
                    fontBrush.Color = MapColor("#009933");
                    phillipsHue.lblHueMessage.Foreground = fontBrush;

                    if (Config.LightSettings.Hue.UseActivityStatus)
                    {
                        phillipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Collapsed;
                        phillipsHue.pnlHueActivityStatuses.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        phillipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                        phillipsHue.pnlHueActivityStatuses.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error Getting Hue Lights");
                    _diagClient.TrackException(ex);
                    phillipsHue.lblHueMessage.Visibility = Visibility.Visible;
                    phillipsHue.pnlHueData.Visibility = Visibility.Collapsed;
                    phillipsHue.lblHueMessage.Text = "Error Occured Connecting to Hue, please try again";
                    fontBrush.Color = MapColor("#ff3300");

                    phillipsHue.btnGetHueLights.IsEnabled = false;
                    phillipsHue.btnGetHueGroups.IsEnabled = false;
                    phillipsHue.lblHueMessage.Foreground = fontBrush;
                }
            }
            phillipsHue.imgHueLoading.Visibility = Visibility.Collapsed;
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
                        phillipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Collapsed;
                        phillipsHue.pnlHueActivityStatuses.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        phillipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                        phillipsHue.pnlHueActivityStatuses.Visibility = Visibility.Collapsed;
                    }

                    if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress))
                    {
                        phillipsHue.hueIpAddress.Text = Config.LightSettings.Hue.HueIpAddress;
                    }

                    if (string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey))
                    {
                        phillipsHue.lblHueMessage.Text = "Missing App Registration, please Login to Hue Cloud or Find Local Bridge";
                        fontBrush.Color = MapColor("#ff3300");
                        phillipsHue.pnlHueData.Visibility = Visibility.Collapsed;
                        phillipsHue.lblHueMessage.Foreground = fontBrush;
                        phillipsHue.btnRegister.IsEnabled = false;
                        return;
                    }

                    if (Config.LightSettings.Hue.UseRemoteApi && string.IsNullOrEmpty(Config.LightSettings.Hue.RemoteBridgeId))
                    {
                        phillipsHue.lblHueMessage.Text = "Bridge Has Not Been Registered, please Login to Hue Cloud";
                        fontBrush.Color = MapColor("#ff3300");
                        phillipsHue.pnlHueData.Visibility = Visibility.Collapsed;
                        phillipsHue.lblHueMessage.Foreground = fontBrush;
                        phillipsHue.btnRegister.IsEnabled = false;
                        return;
                    }

                    if (!IsValidHueIP())
                    {
                        phillipsHue.lblHueMessage.Text = $"IP Address for Bridge Not Valid, please Login to Hue Cloud or Find Local Bridge";
                        fontBrush.Color = MapColor("#ff3300");
                        phillipsHue.pnlHueData.Visibility = Visibility.Collapsed;
                        phillipsHue.btnRegister.IsEnabled = false;
                        phillipsHue.lblHueMessage.Foreground = fontBrush;
                        return;
                    }

                    SyncOptions();

                    if (getLights)
                    {
                        if (Config.LightSettings.Hue.UseRemoteApi)
                        {

                            await _mediator.Send(new PresenceLight.Core.RemoteHueServices.RegisterBridgeCommand());
                            phillipsHue.ddlHueLights.ItemsSource = await _mediator.Send(new Core.RemoteHueServices.GetLightsCommand());

                        }
                        else
                        {
                            phillipsHue.ddlHueLights.ItemsSource = await _mediator.Send(new Core.HueServices.GetLightsCommand());
                        }

                        foreach (var item in phillipsHue.ddlHueLights.Items)
                        {
                            if (item != null)
                            {
                                var light = (Light)item;
                                if ($"id:{light?.Id}" == Config.LightSettings.Hue.SelectedItemId)
                                {
                                    phillipsHue.ddlHueLights.SelectedItem = item;
                                }
                            }
                        }

                        if (phillipsHue.ddlHueLights.SelectedItem == null)
                        {
                            phillipsHue.ddlHueLights.ItemsSource = await _mediator.Send(new Core.HueServices.GetGroupsCommand()).ConfigureAwait(true);

                            foreach (var item in phillipsHue.ddlHueLights.Items)
                            {
                                if (item != null)
                                {
                                    var group = (Q42.HueApi.Models.Groups.Group)item;
                                    if ($"group_id:{group?.Id}" == Config.LightSettings.Hue.SelectedItemId)
                                    {
                                        phillipsHue.ddlHueLights.SelectedItem = item;
                                    }
                                }
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

                        phillipsHue.pnlHueData.Visibility = Visibility.Visible;
                        phillipsHue.lblHueMessage.Text = $"App Registered with {registrationMethod}";
                        fontBrush.Color = MapColor("#009933");
                        phillipsHue.lblHueMessage.Foreground = fontBrush;

                        phillipsHue.btnRegister.IsEnabled = true;

                        if (Config.LightSettings.Hue.UseActivityStatus)
                        {
                            phillipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Collapsed;
                            phillipsHue.pnlHueActivityStatuses.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            phillipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                            phillipsHue.pnlHueActivityStatuses.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured Checking Hue Lights");

                _diagClient.TrackException(ex);
            }
        }

        private bool IsValidHueIP()
        {
            string r2 = @"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b";

            Regex r = new Regex(r2);

            if (string.IsNullOrEmpty(phillipsHue.hueIpAddress.Text.Trim()) || !r.IsMatch(phillipsHue.hueIpAddress.Text.Trim()) || phillipsHue.hueIpAddress.Text.Trim().EndsWith(".", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}

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

        private void cbIsPhilipsEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.Hue.IsEnabled)
            {
                philipsHue.pnlPhilips.Visibility = Visibility.Visible;
                philipsHue.pnlHueApi.Visibility = Visibility.Visible;
            }
            else
            {
                philipsHue.pnlPhilips.Visibility = Visibility.Collapsed;
                philipsHue.pnlHueApi.Visibility = Visibility.Collapsed;
            }

            if (Config.LightSettings.Hue.UseRemoteApi)
            {
                philipsHue.hueIpAddress.IsEnabled = false;
                philipsHue.btnFindBridge.IsEnabled = false;
                philipsHue.btnRegister.IsEnabled = false;
                philipsHue.remoteHueButton.IsEnabled = true;
            }
            else
            {
                philipsHue.remoteHueButton.IsEnabled = false;
                philipsHue.hueIpAddress.IsEnabled = true;
                philipsHue.btnFindBridge.IsEnabled = true;
                philipsHue.btnRegister.IsEnabled = true;
            }

            SyncOptions();
            e.Handled = true;
        }

        private void cbUseRemoteApiChanged(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.Hue.UseRemoteApi)
            {
                philipsHue.hueIpAddress.IsEnabled = false;
                philipsHue.btnFindBridge.IsEnabled = false;
                philipsHue.btnRegister.IsEnabled = false;
                philipsHue.remoteHueButton.IsEnabled = true;
            }
            else
            {
                philipsHue.remoteHueButton.IsEnabled = false;
                philipsHue.hueIpAddress.IsEnabled = true;
                philipsHue.btnFindBridge.IsEnabled = true;
                philipsHue.btnRegister.IsEnabled = true;
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
                philipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Collapsed;
                philipsHue.pnlHueActivityStatuses.Visibility = Visibility.Visible;
            }
            else
            {
                philipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                philipsHue.pnlHueActivityStatuses.Visibility = Visibility.Collapsed;
            }
            SyncOptions();
            e.Handled = true;
        }

        private void cbHueIsDisabledChange(object sender, RoutedEventArgs e)
        {
            var userControl = (PresenceLight.Controls.PhilipsHue)this.FindName("philipsHue");

            CheckBox cb = e.Source as CheckBox ?? throw new ArgumentException("Check Box Not Found");
            var cbName = cb.Name.Replace("Disabled", "Colour");
            var colorpicker = (Xceed.Wpf.Toolkit.ColorPicker)userControl.FindName(cbName);

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
                    philipsHue.hueIpAddress.Text = bridgeIp;
                    Config.LightSettings.Hue.HueApiKey = apiKey;
                    Config.LightSettings.Hue.RemoteBridgeId = bridgeId;
                    Config.LightSettings.Hue.HueIpAddress = bridgeIp;

                    await _settingsService.SaveSettings(Config);

                    philipsHue.ddlHueLights.ItemsSource = await _mediator.Send(new Core.RemoteHueServices.GetLightsCommand());
                    SyncOptions();

                    SolidColorBrush fontBrush = new SolidColorBrush();
                    philipsHue.pnlHueData.Visibility = Visibility.Visible;
                    philipsHue.lblHueMessage.Text = "App Registered with Bridge";
                    fontBrush.Color = MapColor("#009933");
                    philipsHue.lblHueMessage.Foreground = fontBrush;

                    philipsHue.imgHueLoading.Visibility = Visibility.Collapsed;
                    philipsHue.lblHueMessage.Visibility = Visibility.Visible;

                    if (Config.LightSettings.Hue.UseActivityStatus)
                    {
                        philipsHue.pnlHueActivityStatuses.Visibility = Visibility.Visible;
                        philipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        philipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                        philipsHue.pnlHueActivityStatuses.Visibility = Visibility.Collapsed;
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

                philipsHue.hueIpAddress.Text = Config.LightSettings.Hue.HueIpAddress;

                if (!string.IsNullOrEmpty(philipsHue.hueIpAddress.Text))
                {
                    philipsHue.btnRegister.IsEnabled = true;
                }
                else
                {
                    philipsHue.btnRegister.IsEnabled = false;
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
            MessageBox.Show("Please press the sync button on your Philips Hue Bridge");

            SolidColorBrush fontBrush = new SolidColorBrush();

            try
            {
                philipsHue.imgHueLoading.Visibility = Visibility.Visible;
                philipsHue.lblHueMessage.Visibility = Visibility.Collapsed;
                philipsHue.pnlHueData.Visibility = Visibility.Collapsed;
                Config.LightSettings.Hue.HueApiKey = await _mediator.Send(new Core.HueServices.RegisterBridgeCommand()).ConfigureAwait(true);
                philipsHue.ddlHueLights.ItemsSource = await _mediator.Send(new Core.HueServices.GetLightsCommand()).ConfigureAwait(true);
                SyncOptions();
                philipsHue.pnlHueData.Visibility = Visibility.Visible;
                philipsHue.imgHueLoading.Visibility = Visibility.Collapsed;
                philipsHue.lblHueMessage.Visibility = Visibility.Visible;

                if (Config.LightSettings.Hue.UseActivityStatus)
                { }
                else
                {
                    philipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                _diagClient.TrackException(ex);
                _logger.LogError(ex, "Error Occurred Registering Hue Bridge");
                philipsHue.lblHueMessage.Text = "Error Occured registering bridge, please try again";
                fontBrush.Color = MapColor("#ff3300");
                philipsHue.lblHueMessage.Foreground = fontBrush;
            }

            if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey))
            {
                philipsHue.lblHueMessage.Text = "App Registered with Bridge";
                fontBrush.Color = MapColor("#009933");
                philipsHue.lblHueMessage.Foreground = fontBrush;
            }

            CheckHue(true);
        }

        private void ddlHueLights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (philipsHue.ddlHueLights.SelectedItem != null)
            {
                // Get whether item is group or light
                if (philipsHue.ddlHueLights.SelectedItem.GetType() == typeof(Q42.HueApi.Models.Groups.Group))
                {
                    Config.LightSettings.Hue.SelectedItemId = $"group_id:{((Q42.HueApi.Models.Groups.Group)philipsHue.ddlHueLights.SelectedItem).Id}";
                    philipsHue.hueItemType.Content = "Groups";
                }

                if (philipsHue.ddlHueLights.SelectedItem.GetType() == typeof(Light))
                {
                    Config.LightSettings.Hue.SelectedItemId = $"id:{((Light)philipsHue.ddlHueLights.SelectedItem).Id}";
                    philipsHue.hueItemType.Content = "Lights";
                }

                SyncOptions();
            }
            e.Handled = true;
        }

        private async void SaveHue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                philipsHue.btnHue.IsEnabled = false;
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
                philipsHue.lblHueSaved.Visibility = Visibility.Visible;
                philipsHue.btnHue.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured Saving Hue Settings");
                _diagClient.TrackException(ex);
            }
        }

        private async void CheckHue_Click(object sender, RoutedEventArgs e)
        {
            philipsHue.imgHueLoading.Visibility = Visibility.Visible;
            philipsHue.pnlHueData.Visibility = Visibility.Collapsed;
            philipsHue.lblHueMessage.Visibility = Visibility.Collapsed;
            SolidColorBrush fontBrush = new SolidColorBrush();

            if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey))
            {
                try
                {
                    SyncOptions();
                    if (((System.Windows.Controls.Button)e.Source).Name == "btnGetHueGroups")
                    {
                        philipsHue.ddlHueLights.ItemsSource = await _mediator.Send(new Core.HueServices.GetGroupsCommand());
                        philipsHue.hueItemType.Content = "Groups";
                    }
                    else
                    {
                        philipsHue.ddlHueLights.ItemsSource = await _mediator.Send(new Core.HueServices.GetLightsCommand());
                        philipsHue.hueItemType.Content = "Lights";

                    }

                    philipsHue.lblHueMessage.Visibility = Visibility.Visible;
                    philipsHue.pnlHueData.Visibility = Visibility.Visible;
                    philipsHue.lblHueMessage.Text = "Connected to Hue";

                    philipsHue.btnGetHueLights.IsEnabled = true;
                    philipsHue.btnGetHueGroups.IsEnabled = true;
                    fontBrush.Color = MapColor("#009933");
                    philipsHue.lblHueMessage.Foreground = fontBrush;

                    if (Config.LightSettings.Hue.UseActivityStatus)
                    {
                        philipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Collapsed;
                        philipsHue.pnlHueActivityStatuses.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        philipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                        philipsHue.pnlHueActivityStatuses.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error Getting Hue Lights");
                    _diagClient.TrackException(ex);
                    philipsHue.lblHueMessage.Visibility = Visibility.Visible;
                    philipsHue.pnlHueData.Visibility = Visibility.Collapsed;
                    philipsHue.lblHueMessage.Text = "Error Occured Connecting to Hue, please try again";
                    fontBrush.Color = MapColor("#ff3300");

                    philipsHue.btnGetHueLights.IsEnabled = false;
                    philipsHue.btnGetHueGroups.IsEnabled = false;
                    philipsHue.lblHueMessage.Foreground = fontBrush;
                }
            }
            philipsHue.imgHueLoading.Visibility = Visibility.Collapsed;
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
                        philipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Collapsed;
                        philipsHue.pnlHueActivityStatuses.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        philipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                        philipsHue.pnlHueActivityStatuses.Visibility = Visibility.Collapsed;
                    }

                    if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress))
                    {
                        philipsHue.hueIpAddress.Text = Config.LightSettings.Hue.HueIpAddress;
                    }

                    if (string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey))
                    {
                        philipsHue.lblHueMessage.Text = "Missing App Registration, please Login to Hue Cloud or Find Local Bridge";
                        fontBrush.Color = MapColor("#ff3300");
                        philipsHue.pnlHueData.Visibility = Visibility.Collapsed;
                        philipsHue.lblHueMessage.Foreground = fontBrush;
                        philipsHue.btnRegister.IsEnabled = false;
                        return;
                    }

                    if (Config.LightSettings.Hue.UseRemoteApi && string.IsNullOrEmpty(Config.LightSettings.Hue.RemoteBridgeId))
                    {
                        philipsHue.lblHueMessage.Text = "Bridge Has Not Been Registered, please Login to Hue Cloud";
                        fontBrush.Color = MapColor("#ff3300");
                        philipsHue.pnlHueData.Visibility = Visibility.Collapsed;
                        philipsHue.lblHueMessage.Foreground = fontBrush;
                        philipsHue.btnRegister.IsEnabled = false;
                        return;
                    }

                    if (!IsValidHueIP())
                    {
                        philipsHue.lblHueMessage.Text = $"IP Address for Bridge Not Valid, please Login to Hue Cloud or Find Local Bridge";
                        fontBrush.Color = MapColor("#ff3300");
                        philipsHue.pnlHueData.Visibility = Visibility.Collapsed;
                        philipsHue.btnRegister.IsEnabled = false;
                        philipsHue.lblHueMessage.Foreground = fontBrush;
                        return;
                    }

                    SyncOptions();

                    if (getLights)
                    {
                        if (Config.LightSettings.Hue.UseRemoteApi)
                        {

                            await _mediator.Send(new PresenceLight.Core.RemoteHueServices.RegisterBridgeCommand());
                            philipsHue.ddlHueLights.ItemsSource = await _mediator.Send(new Core.RemoteHueServices.GetLightsCommand());

                        }
                        else
                        {
                            philipsHue.ddlHueLights.ItemsSource = await _mediator.Send(new Core.HueServices.GetLightsCommand());
                        }

                        foreach (var item in philipsHue.ddlHueLights.Items)
                        {
                            if (item != null)
                            {
                                var light = (Light)item;
                                if ($"id:{light?.Id}" == Config.LightSettings.Hue.SelectedItemId)
                                {
                                    philipsHue.ddlHueLights.SelectedItem = item;
                                }
                            }
                        }

                        if (philipsHue.ddlHueLights.SelectedItem == null)
                        {
                            philipsHue.ddlHueLights.ItemsSource = await _mediator.Send(new Core.HueServices.GetGroupsCommand()).ConfigureAwait(true);

                            foreach (var item in philipsHue.ddlHueLights.Items)
                            {
                                if (item != null)
                                {
                                    var group = (Q42.HueApi.Models.Groups.Group)item;
                                    if ($"group_id:{group?.Id}" == Config.LightSettings.Hue.SelectedItemId)
                                    {
                                        philipsHue.ddlHueLights.SelectedItem = item;
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

                        philipsHue.pnlHueData.Visibility = Visibility.Visible;
                        philipsHue.lblHueMessage.Text = $"App Registered with {registrationMethod}";
                        fontBrush.Color = MapColor("#009933");
                        philipsHue.lblHueMessage.Foreground = fontBrush;

                        philipsHue.btnRegister.IsEnabled = true;

                        if (Config.LightSettings.Hue.UseActivityStatus)
                        {
                            philipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Collapsed;
                            philipsHue.pnlHueActivityStatuses.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            philipsHue.pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                            philipsHue.pnlHueActivityStatuses.Visibility = Visibility.Collapsed;
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

            if (string.IsNullOrEmpty(philipsHue.hueIpAddress.Text.Trim()) || !r.IsMatch(philipsHue.hueIpAddress.Text.Trim()) || philipsHue.hueIpAddress.Text.Trim().EndsWith(".", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}

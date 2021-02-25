using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

using PresenceLight.Core;

using Q42.HueApi;

namespace PresenceLight
{

    public partial class HuePage
    {
        private MainWindowModern parentWindow;
        public HuePage()
        {
            InitializeComponent();
            parentWindow = Application.Current.Windows.OfType<MainWindowModern>().FirstOrDefault();
            InitializeComponent();
        }


        private void cbIsPhillipsEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (parentWindow.Config.LightSettings.Hue.IsEnabled)
            {
                pnlPhillips.Visibility = Visibility.Visible;
                pnlHueApi.Visibility = Visibility.Visible;
            }
            else
            {
                pnlPhillips.Visibility = Visibility.Collapsed;
                pnlHueApi.Visibility = Visibility.Collapsed;
            }

            if (parentWindow.Config.LightSettings.Hue.UseRemoteApi)
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

            parentWindow.SyncOptions();
            e.Handled = true;
        }

        private void cbUseRemoteApiChanged(object sender, RoutedEventArgs e)
        {
            if (parentWindow.Config.LightSettings.Hue.UseRemoteApi)
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

            if (previousRemoteFlag != parentWindow.Config.LightSettings.Hue.UseRemoteApi)
            {
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
                MessageBox.Show("You toggled Use Remote Api, if this was intentional, please save.");
            }
            previousRemoteFlag = parentWindow.Config.LightSettings.Hue.UseRemoteApi;
            parentWindow.SyncOptions();
            e.Handled = true;
        }

        private void cbUseHueActivityStatus(object sender, RoutedEventArgs e)
        {
            if (parentWindow.Config.LightSettings.Hue.UseActivityStatus)
            {
                pnlHueAvailableStatuses.Visibility = Visibility.Collapsed;
                pnlHueActivityStatuses.Visibility = Visibility.Visible;
            }
            else
            {
                pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                pnlHueActivityStatuses.Visibility = Visibility.Collapsed;
            }
            parentWindow.SyncOptions();
            e.Handled = true;
        }

        private void cbHueIsDisabledChange(object sender, RoutedEventArgs e)
        {
            CheckBox cb = e.Source as CheckBox ?? throw new ArgumentException("Check Box Not Found");
            var cbName = cb.Name.Replace("Disabled", "Colour");
            var colorpicker = (Xceed.Wpf.Toolkit.ColorPicker)this.FindName(cbName);

            colorpicker.IsEnabled = !cb.IsChecked.Value;

            parentWindow.SyncOptions();
            e.Handled = true;
        }

        private async void HueApiKey_Get(object sender, RoutedEventArgs e)
        {
            try
            {
                var (bridgeId, apiKey, bridgeIp) = await parentWindow._mediator.Send(new Core.RemoteHueServices.RegisterBridgeCommand());
                if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(bridgeId) && !string.IsNullOrEmpty(bridgeIp))
                {
                    hueIpAddress.Text = bridgeIp;
                    parentWindow.Config.LightSettings.Hue.HueApiKey = apiKey;
                    parentWindow.Config.LightSettings.Hue.RemoteBridgeId = bridgeId;
                    parentWindow.Config.LightSettings.Hue.HueIpAddress = bridgeIp;

                    await parentWindow._settingsService.SaveSettings(parentWindow.Config);

                    ddlHueLights.ItemsSource = await parentWindow._mediator.Send(new Core.RemoteHueServices.GetLightsCommand());
                    parentWindow.SyncOptions();

                    SolidColorBrush fontBrush = new SolidColorBrush();
                    pnlHueData.Visibility = Visibility.Visible;
                    lblHueMessage.Text = "App Registered with Bridge";
                    fontBrush.Color = parentWindow.MapColor("#009933");
                    lblHueMessage.Foreground = fontBrush;

                    imgHueLoading.Visibility = Visibility.Collapsed;
                    lblHueMessage.Visibility = Visibility.Visible;

                    if (parentWindow.Config.LightSettings.Hue.UseActivityStatus)
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
                parentWindow._logger.LogError(ex, "Error Occured Getting Hue Api Key");
                parentWindow._diagClient.TrackException(ex);
            }
        }

        private async void FindBridge_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                parentWindow.Config.LightSettings.Hue.HueIpAddress = await parentWindow._mediator.Send(new Core.HueServices.FindBridgeCommand()).ConfigureAwait(true);

                hueIpAddress.Text = parentWindow.Config.LightSettings.Hue.HueIpAddress;

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
                parentWindow._logger.LogError(ex, "Error Occured Finding Hue Bridge");
                parentWindow._diagClient.TrackException(ex);
            }
        }

        private void HueIpAddress_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (((TextBox)e.OriginalSource).Text.Trim() != ((TextBox)e.Source).Text.Trim())
            {
                if (parentWindow.Config != null)
                {
                    parentWindow.Config.LightSettings.Hue.HueApiKey = String.Empty;
                }
                parentWindow.SyncOptions();
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
                parentWindow.Config.LightSettings.Hue.HueApiKey = await parentWindow._mediator.Send(new Core.HueServices.RegisterBridgeCommand()).ConfigureAwait(true);
                ddlHueLights.ItemsSource = await parentWindow._mediator.Send(new Core.HueServices.GetLightsCommand()).ConfigureAwait(true);
                parentWindow.SyncOptions();
                pnlHueData.Visibility = Visibility.Visible;
                imgHueLoading.Visibility = Visibility.Collapsed;
                lblHueMessage.Visibility = Visibility.Visible;

                if (parentWindow.Config.LightSettings.Hue.UseActivityStatus)
                { }
                else
                {
                    pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                parentWindow._diagClient.TrackException(ex);
                parentWindow._logger.LogError(ex, "Error Occurred Registering Hue Bridge");
                lblHueMessage.Text = "Error Occured registering bridge, please try again";
                fontBrush.Color = parentWindow.MapColor("#ff3300");
                lblHueMessage.Foreground = fontBrush;
            }

            if (!string.IsNullOrEmpty(parentWindow.Config.LightSettings.Hue.HueApiKey))
            {
                lblHueMessage.Text = "App Registered with Bridge";
                fontBrush.Color = parentWindow.MapColor("#009933");
                lblHueMessage.Foreground = fontBrush;
            }

            CheckHue(true);
        }

        private void ddlHueLights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddlHueLights.SelectedItem != null)
            {
                // Get whether item is group or light
                if (ddlHueLights.SelectedItem.GetType() == typeof(Q42.HueApi.Models.Groups.Group))
                {
                    parentWindow.Config.LightSettings.Hue.SelectedItemId = $"group_id:{((Q42.HueApi.Models.Groups.Group)ddlHueLights.SelectedItem).Id}";
                    hueItemType.Content = "Groups";
                }

                if (ddlHueLights.SelectedItem.GetType() == typeof(Light))
                {
                    parentWindow.Config.LightSettings.Hue.SelectedItemId = $"id:{((Light)ddlHueLights.SelectedItem).Id}";
                    hueItemType.Content = "Lights";
                }

                parentWindow.SyncOptions();
            }
            e.Handled = true;
        }

        private async void SaveHue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnHue.IsEnabled = false;
                parentWindow.Config = Helpers.CleanColors(parentWindow.Config);
                await parentWindow._settingsService.SaveSettings(parentWindow.Config).ConfigureAwait(true);

                if (parentWindow.Config.LightSettings.Hue.UseRemoteApi)
                {
                    await parentWindow._mediator.Send(new Core.RemoteHueServices.InitializeCommand { Options = parentWindow.Config });
                }
                else
                {
                    await parentWindow._mediator.Send(new Core.HueServices.InitializeCommand { Request = parentWindow.Config });
                }

                CheckHue(false);
                lblHueSaved.Visibility = Visibility.Visible;
                btnHue.IsEnabled = true;
            }
            catch (Exception ex)
            {
                parentWindow._logger.LogError(ex, "Error Occured Saving Hue Settings");
                parentWindow._diagClient.TrackException(ex);
            }
        }

        private async void CheckHue_Click(object sender, RoutedEventArgs e)
        {
            imgHueLoading.Visibility = Visibility.Visible;
            pnlHueData.Visibility = Visibility.Collapsed;
            lblHueMessage.Visibility = Visibility.Collapsed;
            SolidColorBrush fontBrush = new SolidColorBrush();

            if (!string.IsNullOrEmpty(parentWindow.Config.LightSettings.Hue.HueApiKey))
            {
                try
                {
                    parentWindow.SyncOptions();
                    if (((System.Windows.Controls.Button)e.Source).Name == "btnGetHueGroups")
                    {
                        ddlHueLights.ItemsSource = await parentWindow._mediator.Send(new Core.HueServices.GetGroupsCommand());
                        hueItemType.Content = "Groups";
                    }
                    else
                    {
                        ddlHueLights.ItemsSource = await parentWindow._mediator.Send(new Core.HueServices.GetLightsCommand());
                        hueItemType.Content = "Lights";

                    }

                    lblHueMessage.Visibility = Visibility.Visible;
                    pnlHueData.Visibility = Visibility.Visible;
                    lblHueMessage.Text = "Connected to Hue";

                    btnGetHueLights.IsEnabled = true;
                    btnGetHueGroups.IsEnabled = true;
                    fontBrush.Color = parentWindow.MapColor("#009933");
                    lblHueMessage.Foreground = fontBrush;

                    if (parentWindow.Config.LightSettings.Hue.UseActivityStatus)
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
                catch (Exception ex)
                {
                    parentWindow._logger.LogError(ex, "Error Getting Hue Lights");
                    parentWindow._diagClient.TrackException(ex);
                    lblHueMessage.Visibility = Visibility.Visible;
                    pnlHueData.Visibility = Visibility.Collapsed;
                    lblHueMessage.Text = "Error Occured Connecting to Hue, please try again";
                    fontBrush.Color = parentWindow.MapColor("#ff3300");

                    btnGetHueLights.IsEnabled = false;
                    btnGetHueGroups.IsEnabled = false;
                    lblHueMessage.Foreground = fontBrush;
                }
            }
            imgHueLoading.Visibility = Visibility.Collapsed;
        }

        private async void CheckHue(bool getLights)
        {
            try
            {
                if (parentWindow.Config != null)
                {
                    SolidColorBrush fontBrush = new SolidColorBrush();

                    if (parentWindow.Config.LightSettings.Hue.UseActivityStatus)
                    {
                        pnlHueAvailableStatuses.Visibility = Visibility.Collapsed;
                        pnlHueActivityStatuses.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                        pnlHueActivityStatuses.Visibility = Visibility.Collapsed;
                    }

                    if (!string.IsNullOrEmpty(parentWindow.Config.LightSettings.Hue.HueIpAddress))
                    {
                        hueIpAddress.Text = parentWindow.Config.LightSettings.Hue.HueIpAddress;
                    }

                    if (string.IsNullOrEmpty(parentWindow.Config.LightSettings.Hue.HueApiKey))
                    {
                        lblHueMessage.Text = "Missing App Registration, please Login to Hue Cloud or Find Local Bridge";
                        fontBrush.Color = parentWindow.MapColor("#ff3300");
                        pnlHueData.Visibility = Visibility.Collapsed;
                        lblHueMessage.Foreground = fontBrush;
                        btnRegister.IsEnabled = false;
                        return;
                    }

                    if (parentWindow.Config.LightSettings.Hue.UseRemoteApi && string.IsNullOrEmpty(parentWindow.Config.LightSettings.Hue.RemoteBridgeId))
                    {
                        lblHueMessage.Text = "Bridge Has Not Been Registered, please Login to Hue Cloud";
                        fontBrush.Color = parentWindow.MapColor("#ff3300");
                        pnlHueData.Visibility = Visibility.Collapsed;
                        lblHueMessage.Foreground = fontBrush;
                        btnRegister.IsEnabled = false;
                        return;
                    }

                    if (!IsValidHueIP())
                    {
                        lblHueMessage.Text = $"IP Address for Bridge Not Valid, please Login to Hue Cloud or Find Local Bridge";
                        fontBrush.Color = parentWindow.MapColor("#ff3300");
                        pnlHueData.Visibility = Visibility.Collapsed;
                        btnRegister.IsEnabled = false;
                        lblHueMessage.Foreground = fontBrush;
                        return;
                    }

                    parentWindow.SyncOptions();

                    if (getLights)
                    {
                        if (parentWindow.Config.LightSettings.Hue.UseRemoteApi)
                        {

                            await parentWindow._mediator.Send(new PresenceLight.Core.RemoteHueServices.RegisterBridgeCommand());
                            ddlHueLights.ItemsSource = await parentWindow._mediator.Send(new Core.RemoteHueServices.GetLightsCommand());

                        }
                        else
                        {
                            ddlHueLights.ItemsSource = await parentWindow._mediator.Send(new Core.HueServices.GetLightsCommand());
                        }

                        foreach (var item in ddlHueLights.Items)
                        {
                            if (item != null)
                            {
                                var light = (Light)item;
                                if ($"id:{light?.Id}" == parentWindow.Config.LightSettings.Hue.SelectedItemId)
                                {
                                    ddlHueLights.SelectedItem = item;
                                }
                            }
                        }

                        if (ddlHueLights.SelectedItem == null)
                        {
                            ddlHueLights.ItemsSource = await parentWindow._mediator.Send(new Core.HueServices.GetGroupsCommand()).ConfigureAwait(true);

                            foreach (var item in ddlHueLights.Items)
                            {
                                if (item != null)
                                {
                                    var group = (Q42.HueApi.Models.Groups.Group)item;
                                    if ($"group_id:{group?.Id}" == parentWindow.Config.LightSettings.Hue.SelectedItemId)
                                    {
                                        ddlHueLights.SelectedItem = item;
                                    }
                                }
                            }
                        }

                        string registrationMethod;
                        if (parentWindow.Config.LightSettings.Hue.UseRemoteApi)
                        {

                            registrationMethod = "Hue Cloud";
                        }
                        else
                        {
                            registrationMethod = "Local Bridge";
                        }

                        pnlHueData.Visibility = Visibility.Visible;
                        lblHueMessage.Text = $"App Registered with {registrationMethod}";
                        fontBrush.Color = parentWindow.MapColor("#009933");
                        lblHueMessage.Foreground = fontBrush;

                        btnRegister.IsEnabled = true;

                        if (parentWindow.Config.LightSettings.Hue.UseActivityStatus)
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
                parentWindow._logger.LogError(ex, "Error Occured Checking Hue Lights");
                parentWindow._diagClient.TrackException(ex);
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
    }
}

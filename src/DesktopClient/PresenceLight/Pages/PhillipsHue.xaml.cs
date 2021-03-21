using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PresenceLight.Core;
using PresenceLight.Services;

using Q42.HueApi;

namespace PresenceLight.Pages
{
    /// <summary>
    /// Interaction logic forxaml
    /// </summary>
    public partial class PhillipsHue 
    {
        public bool previousRemoteFlag;

        private MediatR.IMediator _mediator;

        ILogger _logger;
        public PhillipsHue()
        {
            _mediator = App.ServiceProvider.GetRequiredService<MediatR.IMediator>();

            _logger = App.ServiceProvider.GetRequiredService<ILogger<Yeelight>>();

            InitializeComponent();
          
            cbIsPhillipsEnabled.IsChecked = SettingsHandlerBase.Config.LightSettings.Hue.IsEnabled;

            if (SettingsHandlerBase.Config.LightSettings.Hue.IsEnabled)
            {

                pnlPhillips.Visibility = Visibility.Visible;
                pnlHueApi.Visibility = Visibility.Visible;
                SettingsHandlerBase.SyncOptions();
            }
            else
            {
                pnlPhillips.Visibility = Visibility.Collapsed;
                pnlHueApi.Visibility = Visibility.Collapsed;
            }
            CheckHue(true);
        }

        #region Hue Panel

        private void cbIsPhillipsEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (SettingsHandlerBase.Config.LightSettings.Hue.IsEnabled)
            {
                pnlPhillips.Visibility = Visibility.Visible;
                pnlHueApi.Visibility = Visibility.Visible;
            }
            else
            {
                pnlPhillips.Visibility = Visibility.Collapsed;
                pnlHueApi.Visibility = Visibility.Collapsed;
            }

            if (SettingsHandlerBase.Config.LightSettings.Hue.UseRemoteApi)
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

            SettingsHandlerBase.SyncOptions();
            e.Handled = true;
        }

        private void cbUseRemoteApiChanged(object sender, RoutedEventArgs e)
        {
            if (SettingsHandlerBase.Config.LightSettings.Hue.UseRemoteApi)
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

            if (previousRemoteFlag != SettingsHandlerBase.Config.LightSettings.Hue.UseRemoteApi)
            {
                var parentWindow = Application.Current.Windows.OfType<MainWindowModern>().First();
                MessageBoxHelper.PrepToCenterMessageBoxOnForm(parentWindow);
                MessageBox.Show("You toggled Use Remote Api, if this was intentional, please save.");
            }
            previousRemoteFlag = SettingsHandlerBase.Config.LightSettings.Hue.UseRemoteApi;
            SettingsHandlerBase.SyncOptions();
            e.Handled = true;
        }

        private void cbUseHueActivityStatus(object sender, RoutedEventArgs e)
        {
            if (SettingsHandlerBase.Config.LightSettings.Hue.UseActivityStatus)
            {
                pnlHueAvailableStatuses.Visibility = Visibility.Collapsed;
                pnlHueActivityStatuses.Visibility = Visibility.Visible;
            }
            else
            {
                pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                pnlHueActivityStatuses.Visibility = Visibility.Collapsed;
            }
            SettingsHandlerBase.SyncOptions();
            e.Handled = true;
        }

        private void cbHueIsDisabledChange(object sender, RoutedEventArgs e)
        {
            CheckBox cb = e.Source as CheckBox ?? throw new ArgumentException("Check Box Not Found");
            var cbName = cb.Name.Replace("Disabled", "Colour");
            var colorpicker = (Xceed.Wpf.Toolkit.ColorPicker)this.FindName(cbName);

            colorpicker.IsEnabled = cb.IsChecked == false;

            SettingsHandlerBase.SyncOptions();
            e.Handled = true;
        }

        private async void HueApiKey_Get(object sender, RoutedEventArgs e)
        {
            try
            {
                var (bridgeId, apiKey, bridgeIp) = await _mediator.Send(new Core.RemoteHueServices.RegisterBridgeCommand());
                if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(bridgeId) && !string.IsNullOrEmpty(bridgeIp))
                {
                    hueIpAddress.Text = bridgeIp;
                    SettingsHandlerBase.Config.LightSettings.Hue.HueApiKey = apiKey;
                    SettingsHandlerBase.Config.LightSettings.Hue.RemoteBridgeId = bridgeId;
                    SettingsHandlerBase.Config.LightSettings.Hue.HueIpAddress = bridgeIp;
                    await _mediator.Send(new SaveSettingsCommand());

                    ddlHueLights.ItemsSource = await _mediator.Send(new Core.RemoteHueServices.GetLightsCommand());
                    SettingsHandlerBase.SyncOptions();

                    SolidColorBrush fontBrush = new SolidColorBrush();
                    pnlHueData.Visibility = Visibility.Visible;
                    lblHueMessage.Text = "App Registered with Bridge";
                    fontBrush.Color = "#009933".MapColor();
                    lblHueMessage.Foreground = fontBrush;

                    imgHueLoading.Visibility = Visibility.Collapsed;
                    lblHueMessage.Visibility = Visibility.Visible;

                    if (SettingsHandlerBase.Config.LightSettings.Hue.UseActivityStatus)
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

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured Getting Hue Api Key");
                //TODO:  Revisit this if serilog is insufficient
                //_diagClient.TrackException(ex);
            }
        }

        private async void FindBridge_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                SettingsHandlerBase.Config.LightSettings.Hue.HueIpAddress = await _mediator.Send(new Core.HueServices.FindBridgeCommand()).ConfigureAwait(true);

                hueIpAddress.Text = SettingsHandlerBase.Config.LightSettings.Hue.HueIpAddress;

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
                _logger.LogError(ex, "Error Occured Finding Hue Bridge");
                //TODO:  Revisit this if serilog is insufficient
                //_diagClient.TrackException(ex);
            }
        }

        private void HueIpAddress_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (((TextBox)e.OriginalSource).Text.Trim() != ((TextBox)e.Source).Text.Trim())
            {
                if (SettingsHandlerBase.Config != null)
                {
                    SettingsHandlerBase.Config.LightSettings.Hue.HueApiKey = String.Empty;
                }
                SettingsHandlerBase.SyncOptions();
            }
            CheckHue(false);
            e.Handled = true;
        }

        private async void RegisterBridge_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Application.Current.Windows.OfType<MainWindowModern>().First();
            MessageBoxHelper.PrepToCenterMessageBoxOnForm(parentWindow);

            MessageBox.Show("Please press the sync button on your Phillips Hue Bridge");

            SolidColorBrush fontBrush = new SolidColorBrush();

            try
            {
                imgHueLoading.Visibility = Visibility.Visible;
                lblHueMessage.Visibility = Visibility.Collapsed;
                pnlHueData.Visibility = Visibility.Collapsed;
                SettingsHandlerBase.Config.LightSettings.Hue.HueApiKey = await _mediator.Send(new Core.HueServices.RegisterBridgeCommand()).ConfigureAwait(true);
                ddlHueLights.ItemsSource = await _mediator.Send(new Core.HueServices.GetLightsCommand()).ConfigureAwait(true);
                SettingsHandlerBase.SyncOptions();
                pnlHueData.Visibility = Visibility.Visible;
                imgHueLoading.Visibility = Visibility.Collapsed;
                lblHueMessage.Visibility = Visibility.Visible;

                if (SettingsHandlerBase.Config.LightSettings.Hue.UseActivityStatus)
                { }
                else
                {
                    pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                //TODO:  Revisit this if serilog is insufficient
                //_diagClient.TrackException(ex);
                _logger.LogError(ex, "Error Occurred Registering Hue Bridge");
                lblHueMessage.Text = "Error Occured registering bridge, please try again";
                fontBrush.Color = "#ff3300".MapColor();
                lblHueMessage.Foreground = fontBrush;
            }

            if (!string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.Hue.HueApiKey))
            {
                lblHueMessage.Text = "App Registered with Bridge";
                fontBrush.Color = "#009933".MapColor();
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
                    SettingsHandlerBase.Config.LightSettings.Hue.SelectedItemId = $"group_id:{((Q42.HueApi.Models.Groups.Group)ddlHueLights.SelectedItem).Id}";
                    hueItemType.Content = "Groups";
                }

                if (ddlHueLights.SelectedItem.GetType() == typeof(Light))
                {
                    SettingsHandlerBase.Config.LightSettings.Hue.SelectedItemId = $"id:{((Light)ddlHueLights.SelectedItem).Id}";
                    hueItemType.Content = "Lights";
                }

                SettingsHandlerBase.SyncOptions();
            }
            e.Handled = true;
        }

        private async void SaveHue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnHue.IsEnabled = false;
                await _mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);

                if (SettingsHandlerBase.Config.LightSettings.Hue.UseRemoteApi)
                {
                    await _mediator.Send(new Core.RemoteHueServices.InitializeCommand { Options = SettingsHandlerBase.Config });
                }
                else
                {
                    await _mediator.Send(new Core.HueServices.InitializeCommand { Request = SettingsHandlerBase.Config });
                }

                CheckHue(false);
                lblHueSaved.Visibility = Visibility.Visible;
                btnHue.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured Saving Hue Settings");
                //TODO:  Revisit this if serilog is insufficient
                //_diagClient.TrackException(ex);
            }
        }

        private async void CheckHue_Click(object sender, RoutedEventArgs e)
        {
            imgHueLoading.Visibility = Visibility.Visible;
            pnlHueData.Visibility = Visibility.Collapsed;
            lblHueMessage.Visibility = Visibility.Collapsed;
            SolidColorBrush fontBrush = new SolidColorBrush();

            if (!string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.Hue.HueApiKey))
            {
                try
                {
                    SettingsHandlerBase.SyncOptions();
                    if (((System.Windows.Controls.Button)e.Source).Name == "btnGetHueGroups")
                    {
                        ddlHueLights.ItemsSource = await _mediator.Send(new Core.HueServices.GetGroupsCommand());
                        hueItemType.Content = "Groups";
                    }
                    else
                    {
                        ddlHueLights.ItemsSource = await _mediator.Send(new Core.HueServices.GetLightsCommand());
                        hueItemType.Content = "Lights";

                    }

                    lblHueMessage.Visibility = Visibility.Visible;
                    pnlHueData.Visibility = Visibility.Visible;
                    lblHueMessage.Text = "Connected to Hue";

                    btnGetHueLights.IsEnabled = true;
                    btnGetHueGroups.IsEnabled = true;
                    fontBrush.Color = "#009933".MapColor();
                    lblHueMessage.Foreground = fontBrush;

                    if (SettingsHandlerBase.Config.LightSettings.Hue.UseActivityStatus)
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
                    _logger.LogError(ex, "Error Getting Hue Lights");
                    //TODO:  Revisit this if serilog is insufficient
                    //_diagClient.TrackException(ex);
                    lblHueMessage.Visibility = Visibility.Visible;
                    pnlHueData.Visibility = Visibility.Collapsed;
                    lblHueMessage.Text = "Error Occured Connecting to Hue, please try again";
                    fontBrush.Color = "#ff3300".MapColor();

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
                if (SettingsHandlerBase.Config != null)
                {
                    SolidColorBrush fontBrush = new SolidColorBrush();

                    if (SettingsHandlerBase.Config.LightSettings.Hue.UseActivityStatus)
                    {
                        pnlHueAvailableStatuses.Visibility = Visibility.Collapsed;
                        pnlHueActivityStatuses.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        pnlHueAvailableStatuses.Visibility = Visibility.Visible;
                        pnlHueActivityStatuses.Visibility = Visibility.Collapsed;
                    }

                    if (!string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.Hue.HueIpAddress))
                    {
                        hueIpAddress.Text = SettingsHandlerBase.Config.LightSettings.Hue.HueIpAddress;
                    }

                    if (string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.Hue.HueApiKey))
                    {
                        lblHueMessage.Text = "Missing App Registration, please Login to Hue Cloud or Find Local Bridge";
                        fontBrush.Color = "#ff3300".MapColor();
                        pnlHueData.Visibility = Visibility.Collapsed;
                        lblHueMessage.Foreground = fontBrush;
                        btnRegister.IsEnabled = false;
                        return;
                    }

                    if (SettingsHandlerBase.Config.LightSettings.Hue.UseRemoteApi && string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.Hue.RemoteBridgeId))
                    {
                        lblHueMessage.Text = "Bridge Has Not Been Registered, please Login to Hue Cloud";
                        fontBrush.Color = "#ff3300".MapColor();
                        pnlHueData.Visibility = Visibility.Collapsed;
                        lblHueMessage.Foreground = fontBrush;
                        btnRegister.IsEnabled = false;
                        return;
                    }

                    if (!IsValidHueIP())
                    {
                        lblHueMessage.Text = $"IP Address for Bridge Not Valid, please Login to Hue Cloud or Find Local Bridge";
                        fontBrush.Color = "#ff3300".MapColor();
                        pnlHueData.Visibility = Visibility.Collapsed;
                        btnRegister.IsEnabled = false;
                        lblHueMessage.Foreground = fontBrush;
                        return;
                    }

                    SettingsHandlerBase.SyncOptions();

                    if (getLights)
                    {
                        if (SettingsHandlerBase.Config.LightSettings.Hue.UseRemoteApi)
                        {

                            await _mediator.Send(new PresenceLight.Core.RemoteHueServices.RegisterBridgeCommand());
                            ddlHueLights.ItemsSource = await _mediator.Send(new Core.RemoteHueServices.GetLightsCommand());

                        }
                        else
                        {
                            ddlHueLights.ItemsSource = await _mediator.Send(new Core.HueServices.GetLightsCommand());
                        }

                        foreach (var item in ddlHueLights.Items)
                        {
                            if (item != null)
                            {
                                var light = (Light)item;
                                if ($"id:{light?.Id}" == SettingsHandlerBase.Config.LightSettings.Hue.SelectedItemId)
                                {
                                    ddlHueLights.SelectedItem = item;
                                }
                            }
                        }

                        if (ddlHueLights.SelectedItem == null)
                        {
                            ddlHueLights.ItemsSource = await _mediator.Send(new Core.HueServices.GetGroupsCommand()).ConfigureAwait(true);

                            foreach (var item in ddlHueLights.Items)
                            {
                                if (item != null)
                                {
                                    var group = (Q42.HueApi.Models.Groups.Group)item;
                                    if ($"group_id:{group?.Id}" == SettingsHandlerBase.Config.LightSettings.Hue.SelectedItemId)
                                    {
                                        ddlHueLights.SelectedItem = item;
                                    }
                                }
                            }
                        }

                        string registrationMethod;
                        if (SettingsHandlerBase.Config.LightSettings.Hue.UseRemoteApi)
                        {

                            registrationMethod = "Hue Cloud";
                        }
                        else
                        {
                            registrationMethod = "Local Bridge";
                        }

                        pnlHueData.Visibility = Visibility.Visible;
                        lblHueMessage.Text = $"App Registered with {registrationMethod}";
                        fontBrush.Color = "#009933".MapColor();
                        lblHueMessage.Foreground = fontBrush;

                        btnRegister.IsEnabled = true;

                        if (SettingsHandlerBase.Config.LightSettings.Hue.UseActivityStatus)
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
                _logger.LogError(ex, "Error Occured Checking Hue Lights");

                //TODO:  Revisit this if serilog is insufficient
                //_diagClient.TrackException(ex);
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

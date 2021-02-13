using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using LifxCloud.NET.Models;
using PresenceLight.Telemetry;
using System.Windows.Navigation;
using PresenceLight.Core;
using PresenceLight.Core.LifxServices;

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
                    btnGetLIFXLights.IsEnabled = true;
                    btnGetLIFXGroups.IsEnabled = true;

                    SyncOptions();
                }
                else
                {
                    btnGetLIFXLights.IsEnabled = false;
                    btnGetLIFXGroups.IsEnabled = false;
                }
                this.Activate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured Getting LIFX Token");
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
                _logger.LogError(ex, "Error occured Saving LIFX Settings");
                _diagClient.TrackException(ex);
            }
        }

        private async void CheckLIFX()
        {
            imgLIFXLoading.Visibility = Visibility.Visible;
            pnlLIFXData.Visibility = Visibility.Collapsed;
            lblLIFXMessage.Visibility = Visibility.Collapsed;

            SolidColorBrush fontBrush = new SolidColorBrush();
            try
            {
                if (Config.LightSettings.LIFX.IsEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey) && !string.IsNullOrEmpty(Config.LightSettings.LIFX.SelectedItemId))
                {
                    ddlLIFXLights.ItemsSource = await _mediator.Send(new Core.LifxServices.GetAllLightsCommand()).ConfigureAwait(true);

                    foreach (var item in ddlLIFXLights.Items)
                    {
                        if (item != null)
                        {
                            var light = (Light)item;
                            if ($"id:{light?.Id}" == Config.LightSettings.LIFX.SelectedItemId)
                            {
                                ddlLIFXLights.SelectedItem = item;
                            }
                        }
                    }

                    if (ddlLIFXLights.SelectedItem == null)
                    {
                        ddlLIFXLights.ItemsSource = await _mediator.Send(new GetAllGroupsCommand()).ConfigureAwait(true);

                        foreach (var item in ddlLIFXLights.Items)
                        {
                            if (item != null)
                            {
                                var group = (LifxCloud.NET.Models.Group)item;
                                if ($"group_id:{group?.Id}" == Config.LightSettings.LIFX.SelectedItemId)
                                {
                                    ddlLIFXLights.SelectedItem = item;
                                }
                            }
                        }
                    }

                    if (ddlLIFXLights.SelectedItem != null)
                    {
                        btnGetLIFXLights.IsEnabled = true;
                        btnGetLIFXGroups.IsEnabled = true;

                        pnlLIFXData.Visibility = Visibility.Visible;
                        lblLIFXMessage.Text = "Connected to LIFX Cloud";
                        fontBrush.Color = MapColor("#009933");
                        lblLIFXMessage.Foreground = fontBrush;
                    }

                    if (Config.LightSettings.LIFX.UseActivityStatus)
                    {
                        pnlLIFXAvailableStatuses.Visibility = Visibility.Collapsed;
                        pnlLIFXActivityStatuses.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        pnlLIFXAvailableStatuses.Visibility = Visibility.Visible;
                        pnlLIFXActivityStatuses.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    btnGetLIFXLights.IsEnabled = false;
                    btnGetLIFXGroups.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                _diagClient.TrackException(ex);
                _logger.LogError(ex, "Error occured Checking LIFX");
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
                    Config.LightSettings.LIFX.SelectedItemId = $"group_id:{((LifxCloud.NET.Models.Group)ddlLIFXLights.SelectedItem).Id}";
                }

                if (ddlLIFXLights.SelectedItem.GetType() == typeof(LifxCloud.NET.Models.Light))
                {
                    Config.LightSettings.LIFX.SelectedItemId = $"id:{((LifxCloud.NET.Models.Light)ddlLIFXLights.SelectedItem).Id}";
                }

                SyncOptions();
            }
            e.Handled = true;
        }

        private async void CheckLIFX_Click(object sender, RoutedEventArgs e)
        {
            imgLIFXLoading.Visibility = Visibility.Visible;
            pnlLIFXData.Visibility = Visibility.Collapsed;
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
                        ddlLIFXLights.ItemsSource = await _mediator.Send(new GetAllGroupsCommand()).ConfigureAwait(true);
                    }
                    else
                    {
                        ddlLIFXLights.ItemsSource = await _mediator.Send(new GetAllLightsCommand()).ConfigureAwait(true);
                        
                    }

                    lblLIFXMessage.Visibility = Visibility.Visible;
                    pnlLIFXData.Visibility = Visibility.Visible;
                    lblLIFXMessage.Text = "Connected to LIFX Cloud";

                    btnGetLIFXLights.IsEnabled = true;
                    btnGetLIFXGroups.IsEnabled = true;
                    fontBrush.Color = MapColor("#009933");
                    lblLIFXMessage.Foreground = fontBrush;

                    if (Config.LightSettings.LIFX.UseActivityStatus)
                    {
                        pnlLIFXAvailableStatuses.Visibility = Visibility.Collapsed;
                        pnlLIFXActivityStatuses.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        pnlLIFXAvailableStatuses.Visibility = Visibility.Visible;
                        pnlLIFXActivityStatuses.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error Getting LIFX Lights");
                    _diagClient.TrackException(ex);
                    lblLIFXMessage.Visibility = Visibility.Visible;
                    pnlLIFXData.Visibility = Visibility.Collapsed;
                    lblLIFXMessage.Text = "Error Occured Connecting to LIFX, please try again";
                    fontBrush.Color = MapColor("#ff3300");

                    btnGetLIFXLights.IsEnabled = false;
                    btnGetLIFXGroups.IsEnabled = false;
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

                btnGetLIFXLights.IsEnabled = false;
                btnGetLIFXGroups.IsEnabled = false;
                fontBrush.Color = MapColor("#ff3300");
                lblLIFXMessage.Foreground = fontBrush;

            }

            imgLIFXLoading.Visibility = Visibility.Collapsed;
        }

        private void cbIsLIFXEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.LIFX.IsEnabled)
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

        private void cbUseLIFXActivityStatus(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.LIFX.UseActivityStatus)
            {
                pnlLIFXAvailableStatuses.Visibility = Visibility.Collapsed;
                pnlLIFXActivityStatuses.Visibility = Visibility.Visible;
            }
            else
            {
                pnlLIFXAvailableStatuses.Visibility = Visibility.Visible;
                pnlLIFXActivityStatuses.Visibility = Visibility.Collapsed;
            }
            SyncOptions();
            e.Handled = true;
        }

        private void cbLIFXIsDisabledChange(object sender, RoutedEventArgs e)
        {
            CheckBox cb = e.Source as CheckBox ?? throw new ArgumentException("Check Box Not Found");
            var cbName = cb.Name.Replace("Disabled", "Colour");
            var colorpicker = (Xceed.Wpf.Toolkit.ColorPicker)this.FindName(cbName);
            colorpicker.IsEnabled = !cb.IsChecked.Value;
            SyncOptions();
            e.Handled = true;
        }

        #endregion
    }
}

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
                   lifx. lifxApiKey.Text = accessToken;
                    Config.LightSettings.LIFX.LIFXApiKey = accessToken;
                    lifx.btnGetLIFXLights.IsEnabled = true;
                    lifx.btnGetLIFXGroups.IsEnabled = true;

                    SyncOptions();
                }
                else
                {
                    lifx.btnGetLIFXLights.IsEnabled = false;
                    lifx.btnGetLIFXGroups.IsEnabled = false;
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
                lifx.btnLIFX.IsEnabled = false;
                Config = Helpers.CleanColors(Config);
                await _settingsService.SaveSettings(Config).ConfigureAwait(true);
                lifx.lblLIFXSaved.Visibility = Visibility.Visible;
                lifx.btnLIFX.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured Saving LIFX Settings");
                _diagClient.TrackException(ex);
            }
        }

        private async void CheckLIFX()
        {
            lifx.imgLIFXLoading.Visibility = Visibility.Visible;
            lifx.pnlLIFXData.Visibility = Visibility.Collapsed;
            lifx.lblLIFXMessage.Visibility = Visibility.Collapsed;

            SolidColorBrush fontBrush = new SolidColorBrush();
            try
            {
                if (Config.LightSettings.LIFX.IsEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey) && !string.IsNullOrEmpty(Config.LightSettings.LIFX.SelectedItemId))
                {
                    lifx.ddlLIFXLights.ItemsSource = await _mediator.Send(new Core.LifxServices.GetAllLightsCommand()).ConfigureAwait(true);

                    foreach (var item in lifx.ddlLIFXLights.Items)
                    {
                        if (item != null)
                        {
                            var light = (Light)item;
                            if ($"id:{light?.Id}" == Config.LightSettings.LIFX.SelectedItemId)
                            {
                                lifx.ddlLIFXLights.SelectedItem = item;
                                lifx.lifxItemType.Content = "Lights";
                            }
                        }
                    }

                    if (lifx.ddlLIFXLights.SelectedItem == null)
                    {
                        lifx.ddlLIFXLights.ItemsSource = await _mediator.Send(new GetAllGroupsCommand()).ConfigureAwait(true);

                        foreach (var item in lifx.ddlLIFXLights.Items)
                        {
                            if (item != null)
                            {
                                var group = (LifxCloud.NET.Models.Group)item;
                                if ($"group_id:{group?.Id}" == Config.LightSettings.LIFX.SelectedItemId)
                                {
                                    lifx.ddlLIFXLights.SelectedItem = item;
                                    lifx.lifxItemType.Content = "Groups";
                                }
                            }
                        }
                    }

                    if (lifx.ddlLIFXLights.SelectedItem != null)
                    {
                        lifx.btnGetLIFXLights.IsEnabled = true;
                        lifx.btnGetLIFXGroups.IsEnabled = true;

                        lifx.pnlLIFXData.Visibility = Visibility.Visible;
                        lifx.lblLIFXMessage.Text = "Connected to LIFX Cloud";
                        fontBrush.Color = MapColor("#009933");
                        lifx.lblLIFXMessage.Foreground = fontBrush;
                    }

                    if (Config.LightSettings.LIFX.UseActivityStatus)
                    {
                        lifx.pnlLIFXAvailableStatuses.Visibility = Visibility.Collapsed;
                        lifx.pnlLIFXActivityStatuses.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lifx.pnlLIFXAvailableStatuses.Visibility = Visibility.Visible;
                        lifx.pnlLIFXActivityStatuses.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    lifx.btnGetLIFXLights.IsEnabled = false;
                    lifx.btnGetLIFXGroups.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                _diagClient.TrackException(ex);
                _logger.LogError(ex, "Error occured Checking LIFX");
                lifx.lblLIFXMessage.Text = "Error Occured Connecting to LIFX, please try again";
                fontBrush.Color = MapColor("#ff3300");
                lifx.lblLIFXMessage.Foreground = fontBrush;
            }

            lifx.imgLIFXLoading.Visibility = Visibility.Collapsed;
        }

        private void ddlLIFXLights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lifx.ddlLIFXLights.SelectedItem != null)
            {
                // Get whether item is group or light
                if (lifx.ddlLIFXLights.SelectedItem.GetType() == typeof(LifxCloud.NET.Models.Group))
                {
                    Config.LightSettings.LIFX.SelectedItemId = $"group_id:{((LifxCloud.NET.Models.Group)lifx.ddlLIFXLights.SelectedItem).Id}";
                    lifx.lifxItemType.Content = "Groups";
                }

                if (lifx.ddlLIFXLights.SelectedItem.GetType() == typeof(LifxCloud.NET.Models.Light))
                {
                    Config.LightSettings.LIFX.SelectedItemId = $"id:{((LifxCloud.NET.Models.Light)lifx.ddlLIFXLights.SelectedItem).Id}";
                    lifx.lifxItemType.Content = "Lights";
                }

                SyncOptions();
            }
            e.Handled = true;
        }

        private async void CheckLIFX_Click(object sender, RoutedEventArgs e)
        {
            lifx.imgLIFXLoading.Visibility = Visibility.Visible;
            lifx.pnlLIFXData.Visibility = Visibility.Collapsed;
            lifx.lblLIFXMessage.Visibility = Visibility.Collapsed;
            SolidColorBrush fontBrush = new SolidColorBrush();

            if (!string.IsNullOrEmpty(lifx.lifxApiKey.Text))
            {
                try
                {
                    Config.LightSettings.LIFX.LIFXApiKey = lifx.lifxApiKey.Text;

                    SyncOptions();
                    if (((System.Windows.Controls.Button)e.Source).Name == "btnGetLIFXGroups")
                    {
                        lifx.ddlLIFXLights.ItemsSource = await _mediator.Send(new GetAllGroupsCommand()).ConfigureAwait(true);
                        lifx.lifxItemType.Content = "Groups";
                    }
                    else
                    {
                        lifx.ddlLIFXLights.ItemsSource = await _mediator.Send(new GetAllLightsCommand()).ConfigureAwait(true);
                        lifx.lifxItemType.Content = "Lights";
                    }

                    lifx.lblLIFXMessage.Visibility = Visibility.Visible;
                    lifx.pnlLIFXData.Visibility = Visibility.Visible;
                    lifx.lblLIFXMessage.Text = "Connected to LIFX Cloud";

                    lifx.btnGetLIFXLights.IsEnabled = true;
                    lifx.btnGetLIFXGroups.IsEnabled = true;
                    fontBrush.Color = MapColor("#009933");
                    lifx.lblLIFXMessage.Foreground = fontBrush;

                    if (Config.LightSettings.LIFX.UseActivityStatus)
                    {
                        lifx.pnlLIFXAvailableStatuses.Visibility = Visibility.Collapsed;
                        lifx.pnlLIFXActivityStatuses.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lifx.pnlLIFXAvailableStatuses.Visibility = Visibility.Visible;
                        lifx.pnlLIFXActivityStatuses.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error Getting LIFX Lights");
                    _diagClient.TrackException(ex);
                    lifx.lblLIFXMessage.Visibility = Visibility.Visible;
                    lifx.pnlLIFXData.Visibility = Visibility.Collapsed;
                    lifx.lblLIFXMessage.Text = "Error Occured Connecting to LIFX, please try again";
                    fontBrush.Color = MapColor("#ff3300");

                    lifx.btnGetLIFXLights.IsEnabled = false;
                    lifx.btnGetLIFXGroups.IsEnabled = false;
                    lifx.lblLIFXMessage.Foreground = fontBrush;
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
                lifx.lblLIFXMessage.Inlines.Clear();
                lifx.lblLIFXMessage.Inlines.Add(run1);
                lifx.lblLIFXMessage.Inlines.Add(hyperlink);

                lifx.btnGetLIFXLights.IsEnabled = false;
                lifx.btnGetLIFXGroups.IsEnabled = false;
                fontBrush.Color = MapColor("#ff3300");
                lifx.lblLIFXMessage.Foreground = fontBrush;

            }

            lifx.imgLIFXLoading.Visibility = Visibility.Collapsed;
        }

        private void cbIsLIFXEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.LIFX.IsEnabled)
            {
                lifx.getTokenLink.Visibility = Visibility.Visible;
                lifx.pnlLIFX.Visibility = Visibility.Visible;
            }
            else
            {
                lifx.getTokenLink.Visibility = Visibility.Collapsed;
                lifx.pnlLIFX.Visibility = Visibility.Collapsed;
            }

            SyncOptions();
            e.Handled = true;
        }

        private void cbUseLIFXActivityStatus(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.LIFX.UseActivityStatus)
            {
                lifx.pnlLIFXAvailableStatuses.Visibility = Visibility.Collapsed;
                lifx.pnlLIFXActivityStatuses.Visibility = Visibility.Visible;
            }
            else
            {
                lifx.pnlLIFXAvailableStatuses.Visibility = Visibility.Visible;
                lifx.pnlLIFXActivityStatuses.Visibility = Visibility.Collapsed;
            }
            SyncOptions();
            e.Handled = true;
        }

        private void cbLIFXIsDisabledChange(object sender, RoutedEventArgs e)
        {
            var userControl = (PresenceLight.Controls.LIFX)this.FindName("lifx");

            CheckBox cb = e.Source as CheckBox ?? throw new ArgumentException("Check Box Not Found");
            var cbName = cb.Name.Replace("Disabled", "Colour");
            var colorpicker = (Xceed.Wpf.Toolkit.ColorPicker)userControl.FindName(cbName);
            colorpicker.IsEnabled = !cb.IsChecked.Value;
            SyncOptions();
            e.Handled = true;
        }

        #endregion
    }
}

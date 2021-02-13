using System;
using PresenceLight.Core;
using System.Windows;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using PresenceLight.Telemetry;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace PresenceLight
{
    public partial class MainWindow : Window
    {
        #region Yeelight Panel

        private void cbIsYeelightEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.Yeelight.IsEnabled)
            {
                pnlYeelight.Visibility = Visibility.Visible;
            }
            else
            {
                pnlYeelight.Visibility = Visibility.Collapsed;
            }
            SyncOptions();
            e.Handled = true;
        }

        private async void FindYeelights_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pnlYeelightData.Visibility = Visibility.Collapsed;
                
                var deviceGroup = await _mediator.Send(new Core.YeelightServices.FindLightsCommand()).ConfigureAwait(true);

                ddlYeelightLights.ItemsSource = deviceGroup.ToList();
                pnlYeelightData.Visibility = Visibility.Visible;

                if (Config.LightSettings.Yeelight.UseActivityStatus)
                {
                    pnlYeelightActivityStatuses.Visibility = Visibility.Visible;
                    pnlYeelightAvailableStatuses.Visibility = Visibility.Collapsed;
                }
                else
                {
                    pnlYeelightAvailableStatuses.Visibility = Visibility.Visible;
                    pnlYeelightActivityStatuses.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured Finding YeeLights" );
                _diagClient.TrackException(ex);
            }
        }

        private void ddlYeelightLights_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ddlYeelightLights.SelectedItem != null)
            {
                Config.LightSettings.Yeelight.SelectedItemId = ((YeelightAPI.Device)ddlYeelightLights.SelectedItem).Id;
                SyncOptions();
            }
            e.Handled = true;
        }

        private async void CheckYeelight()
        {
            try
            {
                pnlYeelightData.Visibility = Visibility.Collapsed;
                if (Config != null)
                {
                    SyncOptions();

                     ddlYeelightLights.ItemsSource = await _mediator.Send(new Core.YeelightServices.FindLightsCommand()).ConfigureAwait(true);

                    foreach (var item in ddlYeelightLights.Items)
                    {
                        var light = (YeelightAPI.Device)item;
                        if (light?.Id == Config.LightSettings.Yeelight.SelectedItemId)
                        {
                            ddlYeelightLights.SelectedItem = item;
                        }
                    }
                    ddlYeelightLights.Visibility = Visibility.Visible;
                    pnlYeelightData.Visibility = Visibility.Visible;

                    if (Config.LightSettings.Yeelight.UseActivityStatus)
                    {
                        pnlYeelightActivityStatuses.Visibility = Visibility.Visible;
                        pnlYeelightAvailableStatuses.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        pnlYeelightAvailableStatuses.Visibility = Visibility.Visible;
                        pnlYeelightActivityStatuses.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occured Checking YeeLight");
                _diagClient.TrackException(e);
            }
        }

        private void cbUseYeelightActivityStatus(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.Yeelight.UseActivityStatus)
            {
                pnlYeelightAvailableStatuses.Visibility = Visibility.Collapsed;
                pnlYeelightActivityStatuses.Visibility = Visibility.Visible;
            }
            else
            {
                pnlYeelightAvailableStatuses.Visibility = Visibility.Visible;
                pnlYeelightActivityStatuses.Visibility = Visibility.Collapsed;
            }
            SyncOptions();
            e.Handled = true;
        }

        private void cbYeelightIsDisabledChange(object sender, RoutedEventArgs e)
        {
            CheckBox cb = e.Source as CheckBox ?? throw new ArgumentException("Check Box Not Found");
            var cbName = cb.Name.Replace("Disabled", "Colour");
            var colorpicker = (Xceed.Wpf.Toolkit.ColorPicker)this.FindName(cbName);
            colorpicker.IsEnabled = !cb.IsChecked.Value;
            SyncOptions();
            e.Handled = true;
        }

        private async void SaveYeelight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnYeelight.IsEnabled = false;
                Config = Helpers.CleanColors(Config);
                await _settingsService.SaveSettings(Config).ConfigureAwait(true);

                CheckYeelight();
                lblYeelightSaved.Visibility = Visibility.Visible;
                btnYeelight.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured Saving Yeelight Settings");
                _diagClient.TrackException(ex);
            }
        }
        #endregion
    }
}

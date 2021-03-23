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
                yeelight.pnlYeelight.Visibility = Visibility.Visible;
            }
            else
            {
                yeelight.pnlYeelight.Visibility = Visibility.Collapsed;
            }
            SyncOptions();
            e.Handled = true;
        }

        private async void FindYeelights_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                yeelight.pnlYeelightData.Visibility = Visibility.Collapsed;
                
                var deviceGroup = await _mediator.Send(new Core.YeelightServices.GetLightCommand()).ConfigureAwait(true);

                yeelight.ddlYeelightLights.ItemsSource = deviceGroup.ToList();
                yeelight.pnlYeelightData.Visibility = Visibility.Visible;

                if (Config.LightSettings.Yeelight.UseActivityStatus)
                {
                    yeelight.pnlYeelightActivityStatuses.Visibility = Visibility.Visible;
                    yeelight.pnlYeelightAvailableStatuses.Visibility = Visibility.Collapsed;
                }
                else
                {
                    yeelight.pnlYeelightAvailableStatuses.Visibility = Visibility.Visible;
                    yeelight.pnlYeelightActivityStatuses.Visibility = Visibility.Collapsed;
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
            if (yeelight.ddlYeelightLights.SelectedItem != null)
            {
                Config.LightSettings.Yeelight.SelectedItemId = ((YeelightAPI.Device)yeelight.ddlYeelightLights.SelectedItem).Id;
                SyncOptions();
            }
            e.Handled = true;
        }

        private async void CheckYeelight()
        {
            try
            {
                yeelight.pnlYeelightData.Visibility = Visibility.Collapsed;
                if (Config != null)
                {
                    SyncOptions();

                    yeelight.ddlYeelightLights.ItemsSource = await _mediator.Send(new Core.YeelightServices.GetLightCommand()).ConfigureAwait(true);

                    foreach (var item in yeelight.ddlYeelightLights.Items)
                    {
                        var light = (YeelightAPI.Device)item;
                        if (light?.Id == Config.LightSettings.Yeelight.SelectedItemId)
                        {
                            yeelight.ddlYeelightLights.SelectedItem = item;
                        }
                    }
                    yeelight.ddlYeelightLights.Visibility = Visibility.Visible;
                    yeelight.pnlYeelightData.Visibility = Visibility.Visible;

                    if (Config.LightSettings.Yeelight.UseActivityStatus)
                    {
                        yeelight.pnlYeelightActivityStatuses.Visibility = Visibility.Visible;
                        yeelight.pnlYeelightAvailableStatuses.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        yeelight.pnlYeelightAvailableStatuses.Visibility = Visibility.Visible;
                        yeelight.pnlYeelightActivityStatuses.Visibility = Visibility.Collapsed;
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
                yeelight.pnlYeelightAvailableStatuses.Visibility = Visibility.Collapsed;
                yeelight.pnlYeelightActivityStatuses.Visibility = Visibility.Visible;
            }
            else
            {
                yeelight.pnlYeelightAvailableStatuses.Visibility = Visibility.Visible;
                yeelight.pnlYeelightActivityStatuses.Visibility = Visibility.Collapsed;
            }
            SyncOptions();
            e.Handled = true;
        }

        private void cbYeelightIsDisabledChange(object sender, RoutedEventArgs e)
        {
            var userControl = (PresenceLight.Controls.Yeelight)this.FindName("yeelight");

            CheckBox cb = e.Source as CheckBox ?? throw new ArgumentException("Check Box Not Found");
            var cbName = cb.Name.Replace("Disabled", "Colour");
            var colorpicker = (Xceed.Wpf.Toolkit.ColorPicker)userControl.FindName(cbName);
            colorpicker.IsEnabled = !cb.IsChecked.Value;
            SyncOptions();
            e.Handled = true;
        }

        private async void SaveYeelight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                yeelight.btnYeelight.IsEnabled = false;
                Config = Helpers.CleanColors(Config);
                await _settingsService.SaveSettings(Config).ConfigureAwait(true);

                CheckYeelight();
                yeelight.lblYeelightSaved.Visibility = Visibility.Visible;
                yeelight.btnYeelight.IsEnabled = true;
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

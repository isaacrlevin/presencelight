using System;
using PresenceLight.Core;
using System.Windows;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using PresenceLight.Telemetry;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using PresenceLight.Core.WizServices;

namespace PresenceLight
{
    public partial class MainWindow : Window
    {
        #region Wiz Panel

        private void cbIsWizEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.Wiz.IsEnabled)
            {
                wiz.pnlWiz.Visibility = Visibility.Visible;
            }
            else
            {
                wiz.pnlWiz.Visibility = Visibility.Collapsed;
            }
            SyncOptions();
            e.Handled = true;
        }

        private async void FindWiz_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                wiz.pnlWizData.Visibility = Visibility.Collapsed;
                var lightList = await _mediator.Send(new Core.WizServices.GetLightsCommand()).ConfigureAwait(true);

                wiz.ddlWizLights.ItemsSource = lightList;
                wiz.pnlWizData.Visibility = Visibility.Visible;

                if (Config.LightSettings.Wiz.UseActivityStatus)
                {
                    wiz.pnlWizActivityStatuses.Visibility = Visibility.Visible;
                    wiz.pnlWizAvailableStatuses.Visibility = Visibility.Collapsed;
                }
                else
                {
                    wiz.pnlWizAvailableStatuses.Visibility = Visibility.Visible;
                    wiz.pnlWizActivityStatuses.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured Finding Wiz");
                _diagClient.TrackException(ex);
            }
        }

        private void ddlWizLights_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (wiz.ddlWizLights.SelectedItem != null)
            {
                var selectedWizItem = (WizLight)wiz.ddlWizLights.SelectedItem;
                Config.LightSettings.Wiz.SelectedItemId = selectedWizItem.MacAddress;
                SyncOptions();
            }
            e.Handled = true;
        }

        private async void CheckWiz()
        {
            try
            {
                wiz.pnlWizData.Visibility = Visibility.Collapsed;
                if (Config != null)
                {
                    SyncOptions();

                    wiz.ddlWizLights.ItemsSource = await _mediator.Send(new Core.WizServices.GetLightsCommand()).ConfigureAwait(true);


                    foreach (var item in wiz.ddlWizLights.Items)
                    {
                        var light = (WizLight)item;
                        if (light.MacAddress == Config.LightSettings.Wiz.SelectedItemId)
                        {
                            wiz.ddlWizLights.SelectedItem = item;
                        }
                    }
                    wiz.ddlWizLights.Visibility = Visibility.Visible;
                    wiz.pnlWizData.Visibility = Visibility.Visible;

                    if (Config.LightSettings.Wiz.UseActivityStatus)
                    {
                        wiz.pnlWizActivityStatuses.Visibility = Visibility.Visible;
                        wiz.pnlWizAvailableStatuses.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        wiz.pnlWizAvailableStatuses.Visibility = Visibility.Visible;
                        wiz.pnlWizActivityStatuses.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occured Checking Wiz");
                _diagClient.TrackException(e);
            }
        }

        private void cbUseWizActivityStatus(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.Wiz.UseActivityStatus)
            {
                wiz.pnlWizAvailableStatuses.Visibility = Visibility.Collapsed;
                wiz.pnlWizActivityStatuses.Visibility = Visibility.Visible;
            }
            else
            {
                wiz.pnlWizAvailableStatuses.Visibility = Visibility.Visible;
                wiz.pnlWizActivityStatuses.Visibility = Visibility.Collapsed;
            }
            SyncOptions();
            e.Handled = true;
        }

        private void cbWizIsDisabledChange(object sender, RoutedEventArgs e)
        {
            var userControl = (PresenceLight.Controls.Wiz)this.FindName("wiz");

            CheckBox cb = e.Source as CheckBox ?? throw new ArgumentException("Check Box Not Found");
            var cbName = cb.Name.Replace("Disabled", "Colour");
            var colorpicker = (Xceed.Wpf.Toolkit.ColorPicker)this.FindName(cbName);
            colorpicker.IsEnabled = !cb.IsChecked.Value;
            SyncOptions();
            e.Handled = true;
        }

        private async void SaveWiz_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                wiz.btnWiz.IsEnabled = false;
                Config = Helpers.CleanColors(Config);
                await _settingsService.SaveSettings(Config).ConfigureAwait(true);

                CheckWiz();
                wiz.lblWizSaved.Visibility = Visibility.Visible;
                wiz.btnWiz.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured Saving Wiz Settings");
                _diagClient.TrackException(ex);
            }
        }
        #endregion
    }
}

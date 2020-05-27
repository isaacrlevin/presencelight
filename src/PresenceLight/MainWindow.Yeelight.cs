using System;
using PresenceLight.Core;
using System.Windows;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using PresenceLight.Telemetry;
using System.Linq;

namespace PresenceLight
{
    public partial class MainWindow : Window
    {
        #region Yeelight Panel

        private void cbIsYeelightEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Config.IsYeelightEnabled)
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
            var deviceGroup = await _yeelightService.FindLights();
            ddlYeelightLights.ItemsSource = deviceGroup.ToList();
        }

        private void ddlYeelightLights_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ddlYeelightLights.SelectedItem != null)
            {
                Config.SelectedYeeLightId = ((YeelightAPI.Device)ddlYeelightLights.SelectedItem).Id;
                SyncOptions();
            }
            e.Handled = true;
        }

        private async void CheckYeelightSettings()
        {
            if (Config != null)
            {
                SyncOptions();

                ddlYeelightLights.ItemsSource = await _yeelightService.FindLights();

                foreach (var item in ddlYeelightLights.Items)
                {
                    var light = (YeelightAPI.Device)item;
                    if (light?.Id == Config.SelectedYeeLightId)
                    {
                        ddlYeelightLights.SelectedItem = item;
                    }
                }
                ddlYeelightLights.Visibility = Visibility.Visible;
            }
        }

        #endregion
    }
}

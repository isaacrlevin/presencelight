using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using LifxCloud.NET.Models;
using PresenceLight.Telemetry;
using System.Windows.Navigation;
using PresenceLight.Core;

namespace PresenceLight
{
    public partial class MainWindow : Window
    {

        private void cbIsMqttEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Config.MqttSettings.IsEnabled)
            {
                mqtt.pnlMqtt.Visibility = Visibility.Visible;
            }
            else
            {
                mqtt.pnlMqtt.Visibility = Visibility.Collapsed;
            }

            SyncOptions();
            e.Handled = true;
        }



        private async void btnMqttSettingsSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Config = Helpers.CleanColors(Config);
                await _settingsService.SaveSettings(Config).ConfigureAwait(true);
                mqtt.lblMqttSettingsSaved.Visibility = Visibility.Visible;
                SyncOptions();
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error Occured Saving Custom Api Settings");
                _diagClient.TrackException(ex);
            }
        }
    }
}

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

        private void cbIsCustomApiEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.CustomApi.IsEnabled)
            {
                customapi.pnlCustomApi.Visibility = Visibility.Visible;
            }
            else
            {
                customapi.pnlCustomApi.Visibility = Visibility.Collapsed;
            }

            SyncOptions();
            e.Handled = true;
        }

        private void customApiMethod_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBox sourceComboBox = e.Source as ComboBox ?? throw new ArgumentException("Custom Api Not Found");
            ComboBoxItem selectedItem = (ComboBoxItem)sourceComboBox.SelectedItem;
            string selectedText = selectedItem.Content.ToString() ?? throw new ArgumentException("Custom Api Not Found");

            switch (sourceComboBox.Name)
            {
                case "customApiAvailableMethod":
                    Config.LightSettings.CustomApi.CustomApiAvailable.Method = selectedText;
                    break;
                case "customApiBusyMethod":
                    Config.LightSettings.CustomApi.CustomApiBusy.Method = selectedText;
                    break;
                case "customApiBeRightBackMethod":
                    Config.LightSettings.CustomApi.CustomApiBeRightBack.Method = selectedText;
                    break;
                case "customApiAwayMethod":
                    Config.LightSettings.CustomApi.CustomApiAway.Method = selectedText;
                    break;
                case "customApiDoNotDisturbMethod":
                    Config.LightSettings.CustomApi.CustomApiDoNotDisturb.Method = selectedText;
                    break;
                case "customApiAvailableIdleMethod":
                    Config.LightSettings.CustomApi.CustomApiAvailableIdle.Method = selectedText;
                    break;
                case "customApiOfflineMethod":
                    Config.LightSettings.CustomApi.CustomApiOffline.Method = selectedText;
                    break;
                case "customApiOffMethod":
                    Config.LightSettings.CustomApi.CustomApiOff.Method = selectedText;
                    break;
                case "customApiActivityAvailableMethod":
                    Config.LightSettings.CustomApi.CustomApiActivityAvailable.Method = selectedText;
                    break;
                case "customApiActivityPresentingMethod":
                    Config.LightSettings.CustomApi.CustomApiActivityPresenting.Method = selectedText;
                    break;
                case "customApiActivityInACallMethod":
                    Config.LightSettings.CustomApi.CustomApiActivityInACall.Method = selectedText;
                    break;
                case "customApiActivityInAConferenceCallMethod":
                    Config.LightSettings.CustomApi.CustomApiActivityInAConferenceCall.Method = selectedText;
                    break;
                case "customApiActivityInAMeetingMethod":
                    Config.LightSettings.CustomApi.CustomApiActivityInAMeeting.Method = selectedText;
                    break;
                case "customApiActivityBusyMethod":
                    Config.LightSettings.CustomApi.CustomApiActivityBusy.Method = selectedText;
                    break;
                case "customApiActivityAwayMethod":
                    Config.LightSettings.CustomApi.CustomApiActivityAway.Method = selectedText;
                    break;
                case "customApiActivityBeRightBackMethod":
                    Config.LightSettings.CustomApi.CustomApiActivityBeRightBack.Method = selectedText;
                    break;
                case "customApiActivityDoNotDisturbMethod":
                    Config.LightSettings.CustomApi.CustomApiActivityDoNotDisturb.Method = selectedText;
                    break;
                case "customApiActivityIdleMethod":
                    Config.LightSettings.CustomApi.CustomApiActivityIdle.Method = selectedText;
                    break;
                case "customApiActivityOfflineMethod":
                    Config.LightSettings.CustomApi.CustomApiActivityOffline.Method = selectedText;
                    break;
                case "customApiActivityOffMethod":
                    Config.LightSettings.CustomApi.CustomApiActivityOff.Method = selectedText;
                    break;
                default:
                    break;
            }

            e.Handled = true;
        }

        private async void btnApiSettingsSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Config = Helpers.CleanColors(Config);
                await _settingsService.SaveSettings(Config).ConfigureAwait(true);
                customapi.lblCustomApiSaved.Visibility = Visibility.Visible;
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

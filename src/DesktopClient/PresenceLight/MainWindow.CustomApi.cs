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
            if (Config.LightSettings.Custom.IsCustomApiEnabled)
            {
                pnlCustomApi.Visibility = Visibility.Visible;
            }
            else
            {
                pnlCustomApi.Visibility = Visibility.Collapsed;
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
                    Config.LightSettings.Custom.CustomApiAvailable.Method = selectedText;
                    break;
                case "customApiBusyMethod":
                    Config.LightSettings.Custom.CustomApiBusy.Method = selectedText;
                    break;
                case "customApiBeRightBackMethod":
                    Config.LightSettings.Custom.CustomApiBeRightBack.Method = selectedText;
                    break;
                case "customApiAwayMethod":
                    Config.LightSettings.Custom.CustomApiAway.Method = selectedText;
                    break;
                case "customApiDoNotDisturbMethod":
                    Config.LightSettings.Custom.CustomApiDoNotDisturb.Method = selectedText;
                    break;
                case "customApiAvailableIdleMethod":
                    Config.LightSettings.Custom.CustomApiAvailableIdle.Method = selectedText;
                    break;
                case "customApiOfflineMethod":
                    Config.LightSettings.Custom.CustomApiOffline.Method = selectedText;
                    break;
                case "customApiOffMethod":
                    Config.LightSettings.Custom.CustomApiOff.Method = selectedText;
                    break;
                case "customApiActivityAvailableMethod":
                    Config.LightSettings.Custom.CustomApiActivityAvailable.Method = selectedText;
                    break;
                case "customApiActivityPresentingMethod":
                    Config.LightSettings.Custom.CustomApiActivityPresenting.Method = selectedText;
                    break;
                case "customApiActivityInACallMethod":
                    Config.LightSettings.Custom.CustomApiActivityInACall.Method = selectedText;
                    break;
                case "customApiActivityInAMeetingMethod":
                    Config.LightSettings.Custom.CustomApiActivityInAMeeting.Method = selectedText;
                    break;
                case "customApiActivityBusyMethod":
                    Config.LightSettings.Custom.CustomApiActivityBusy.Method = selectedText;
                    break;
                case "customApiActivityAwayMethod":
                    Config.LightSettings.Custom.CustomApiActivityAway.Method = selectedText;
                    break;
                case "customApiActivityBeRightBackMethod":
                    Config.LightSettings.Custom.CustomApiActivityBeRightBack.Method = selectedText;
                    break;
                case "customApiActivityDoNotDisturbMethod":
                    Config.LightSettings.Custom.CustomApiActivityDoNotDisturb.Method = selectedText;
                    break;
                case "customApiActivityIdleMethod":
                    Config.LightSettings.Custom.CustomApiActivityIdle.Method = selectedText;
                    break;
                case "customApiActivityOfflineMethod":
                    Config.LightSettings.Custom.CustomApiActivityOffline.Method = selectedText;
                    break;
                case "customApiActivityOffMethod":
                    Config.LightSettings.Custom.CustomApiActivityOff.Method = selectedText;
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
                lblCustomApiSaved.Visibility = Visibility.Visible;
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

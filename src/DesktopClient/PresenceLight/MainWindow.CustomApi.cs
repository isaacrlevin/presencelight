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
                    Config.LightSettings.Custom.CustomApiAvailableMethod = selectedText;
                    break;
                case "customApiBusyMethod":
                    Config.LightSettings.Custom.CustomApiBusyMethod = selectedText;
                    break;
                case "customApiBeRightBackMethod":
                    Config.LightSettings.Custom.CustomApiBeRightBackMethod = selectedText;
                    break;
                case "customApiAwayMethod":
                    Config.LightSettings.Custom.CustomApiAwayMethod = selectedText;
                    break;
                case "customApiDoNotDisturbMethod":
                    Config.LightSettings.Custom.CustomApiDoNotDisturbMethod = selectedText;
                    break;
                case "customApiAvailableIdleMethod":
                    Config.LightSettings.Custom.CustomApiAvailableIdleMethod = selectedText;
                    break;
                case "customApiOfflineMethod":
                    Config.LightSettings.Custom.CustomApiOfflineMethod = selectedText;
                    break;
                case "customApiOffMethod":
                    Config.LightSettings.Custom.CustomApiOffMethod = selectedText;
                    break;
                case "customApiActivityAvailableMethod":
                    Config.LightSettings.Custom.CustomApiActivityAvailableMethod = selectedText;
                    break;
                case "customApiActivityPresentingMethod":
                    Config.LightSettings.Custom.CustomApiActivityPresentingMethod = selectedText;
                    break;
                case "customApiActivityInACallMethod":
                    Config.LightSettings.Custom.CustomApiActivityInACallMethod = selectedText;
                    break;
                case "customApiActivityInAMeetingMethod":
                    Config.LightSettings.Custom.CustomApiActivityInAMeetingMethod = selectedText;
                    break;
                case "customApiActivityBusyMethod":
                    Config.LightSettings.Custom.CustomApiActivityBusyMethod = selectedText;
                    break;
                case "customApiActivityAwayMethod":
                    Config.LightSettings.Custom.CustomApiActivityAwayMethod = selectedText;
                    break;
                case "customApiActivityBeRightBackMethod":
                    Config.LightSettings.Custom.CustomApiActivityBeRightBackMethod = selectedText;
                    break;
                case "customApiActivityDoNotDisturbMethod":
                    Config.LightSettings.Custom.CustomApiActivityDoNotDisturbMethod = selectedText;
                    break;
                case "customApiActivityIdleMethod":
                    Config.LightSettings.Custom.CustomApiActivityIdleMethod = selectedText;
                    break;
                case "customApiActivityOfflineMethod":
                    Config.LightSettings.Custom.CustomApiActivityOfflineMethod = selectedText;
                    break;
                case "customApiActivityOffMethod":
                    Config.LightSettings.Custom.CustomApiActivityOffMethod = selectedText;
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
                Helpers.AppendLogger(_logger, "Error Occured Saving Custom Api Settings", ex);
                _diagClient.TrackException(ex);
            }
        }
    }
}

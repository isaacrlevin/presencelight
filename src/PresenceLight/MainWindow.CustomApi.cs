using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using LifxCloud.NET.Models;
using PresenceLight.Telemetry;
using System.Windows.Navigation;

namespace PresenceLight
{
    public partial class MainWindow : Window
    {

        private void cbIsCustomApiEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Config.IsCustomApiEnabled)
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
            ComboBox sourceComboBox = e.Source as ComboBox;
            ComboBoxItem selectedItem = (ComboBoxItem)sourceComboBox.SelectedItem;
            string selectedText = selectedItem.Content.ToString();

            switch (sourceComboBox.Name)
            {
                case "customApiAvailableMethod":
                    Config.CustomApiAvailableMethod = selectedText;
                    break;
                case "customApiBusyMethod":
                    Config.CustomApiBusyMethod = selectedText;
                    break;

                case "customApiBeRightBackMethod":
                    Config.CustomApiBeRightBackMethod = selectedText;
                    break;
                case "customApiAwayMethod":
                    Config.CustomApiAwayMethod = selectedText;
                    break;
                case "customApiDoNotDisturbMethod":
                    Config.CustomApiDoNotDisturbMethod = selectedText;
                    break;
                case "customApiOfflineMethod":
                    Config.CustomApiOfflineMethod = selectedText;
                    break;
                case "customApiOffMethod":
                    Config.CustomApiOffMethod = selectedText;
                    break;
                default:
                    break;
            }

            e.Handled = true;
        }

        private async void btnApiSettingsSave_Click(object sender, RoutedEventArgs e)
        {
            await SettingsService.SaveSettings(Config);
            SyncOptions();
        }

    }

}

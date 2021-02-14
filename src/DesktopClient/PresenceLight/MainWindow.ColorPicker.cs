using System;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Media;
using PresenceLight.Telemetry;
using System.Globalization;
using PresenceLight.Core;

namespace PresenceLight
{
    public partial class MainWindow : Window
    {
        private void SetTeamsPresence_Click(object sender, RoutedEventArgs e)
        {
            lightMode = "Graph";
            lightColors.syncTeamsButton.IsEnabled = false;
            lightColors.syncThemeButton.IsEnabled = true;
        }

        private async void SyncTheme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lightMode = "Theme";
                lightColors.syncTeamsButton.IsEnabled = true;
                lightColors.syncThemeButton.IsEnabled = false;

                var theme = ((SolidColorBrush)SystemParameters.WindowGlassBrush).Color;

                string color = $"#{theme.ToString(CultureInfo.InvariantCulture).Substring(3)}";

                lightColors.lblTheme.Content = $"Theme Color is {color}";
                lightColors.lblTheme.Foreground = (SolidColorBrush)SystemParameters.WindowGlassBrush;
                lightColors.lblTheme.Visibility = Visibility.Visible;

                await SetColor(color).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured Setting Theme Color");
                _diagClient.TrackException(ex);
            }
        }


        private async void SetColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lightColors.ColorGrid.SelectedColor.HasValue)
                {
                    lightMode = "Custom";
                    lightColors.syncTeamsButton.IsEnabled = true;
                    lightColors.syncThemeButton.IsEnabled = true;

                    string color = $"#{lightColors.ColorGrid.HexadecimalString.ToString().Substring(3)}";

                    if (lightMode == "Custom")
                    {
                        await SetColor(color).ConfigureAwait(true);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured Setting Custom Color");
                _diagClient.TrackException(ex);
            }
        }
    }
}

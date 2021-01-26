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
            syncTeamsButton.IsEnabled = false;
            syncThemeButton.IsEnabled = true;
        }

        private async void SyncTheme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lightMode = "Theme";
                syncTeamsButton.IsEnabled = true;
                syncThemeButton.IsEnabled = false;

                var theme = ((SolidColorBrush)SystemParameters.WindowGlassBrush).Color;

                string color = $"#{theme.ToString(CultureInfo.InvariantCulture).Substring(3)}";

                lblTheme.Content = $"Theme Color is {color}";
                lblTheme.Foreground = (SolidColorBrush)SystemParameters.WindowGlassBrush;
                lblTheme.Visibility = Visibility.Visible;

                await SetColor(color).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Helpers.AppendLogger(_logger, "Error occured Setting Theme Color", ex);
                _diagClient.TrackException(ex);
            }
        }


        private async void SetColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ColorGrid.SelectedColor.HasValue)
                {
                    lightMode = "Custom";
                    syncTeamsButton.IsEnabled = true;
                    syncThemeButton.IsEnabled = true;

                    string color = $"#{ColorGrid.HexadecimalString.ToString().Substring(3)}";

                    if (lightMode == "Custom")
                    {
                        await SetColor(color).ConfigureAwait(true);
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.AppendLogger(_logger, "Error occured Setting Custom Color", ex);
                _diagClient.TrackException(ex);
            }
        }
    }
}

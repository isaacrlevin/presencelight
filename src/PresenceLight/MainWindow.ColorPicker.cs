using System;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Media;
using PresenceLight.Telemetry;

namespace PresenceLight
{
    public partial class MainWindow : Window
    {
        private async void SetTeamsPresence_Click(object sender, RoutedEventArgs e)
        {
            lightMode = "Graph";
            syncTeamsButton.IsEnabled = false;
            syncThemeButton.IsEnabled = true;
        }

        private async void SyncTheme_Click(object sender, RoutedEventArgs e)
        {
            lightMode = "Theme";
            syncTeamsButton.IsEnabled = true;
            syncThemeButton.IsEnabled = false;

            var theme = ((SolidColorBrush)SystemParameters.WindowGlassBrush).Color;

            string color = $"#{theme.ToString().Substring(3)}";

            lblTheme.Content = $"Theme Color is {color}";
            lblTheme.Foreground = (SolidColorBrush)SystemParameters.WindowGlassBrush;
            lblTheme.Visibility = Visibility.Visible;

            await SetColor(color);
        }


        private async void SetColor_Click(object sender, RoutedEventArgs e)
        {
            if (ColorGrid.SelectedColor.HasValue)
            {
                lightMode = "Custom";
                syncTeamsButton.IsEnabled = true;
                syncThemeButton.IsEnabled = true;
                savedAvailability = string.Empty;

                string color = $"#{ColorGrid.HexadecimalString.ToString().Substring(3)}";

                if (lightMode == "Custom")
                {
                    await SetColor(color);
                }
            }
        }
    }
}

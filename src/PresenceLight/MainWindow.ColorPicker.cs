using System;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Media;
using LifxCloud.NET.Models;
using PresenceLight.Telemetry;

namespace PresenceLight
{
    public partial class MainWindow : Window
    {
        private async void SyncTheme_Click(object sender, RoutedEventArgs e)
        {
            SignOutButton_Click(null, null);

            var theme = ((SolidColorBrush)SystemParameters.WindowGlassBrush).Color;
            stopGraphPolling = true;
            stopThemePolling = false;
            string color = $"#{theme.ToString().Substring(3)}";

            lblTheme.Content = $"Theme Color is {color}";
            lblTheme.Foreground = (SolidColorBrush)SystemParameters.WindowGlassBrush;
            lblTheme.Visibility = Visibility.Visible;

            if (!string.IsNullOrEmpty(Config.HueApiKey) && !string.IsNullOrEmpty(Config.HueIpAddress) && !string.IsNullOrEmpty(Config.SelectedHueLightId))
            {
                await _hueService.SetColor(color, Config.SelectedHueLightId);
            }

            if (Config.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LIFXApiKey))
            {
                await _lifxService.SetColor(color, (Selector)Config.SelectedLIFXItemId);
            }

            while (true)
            {
                if (stopThemePolling)
                {
                    stopThemePolling = false;
                    return;
                }
                await Task.Delay(Convert.ToInt32(Config.PollingInterval * 1000));
                try
                {
                    theme = ((SolidColorBrush)SystemParameters.WindowGlassBrush).Color;
                    color = $"#{theme.ToString().Substring(3)}";

                    lblTheme.Content = $"Theme Color is {color}";
                    lblTheme.Foreground = (SolidColorBrush)SystemParameters.WindowGlassBrush;
                    lblTheme.Visibility = Visibility.Visible;

                    if (!string.IsNullOrEmpty(Config.HueApiKey) && !string.IsNullOrEmpty(Config.HueIpAddress) && !string.IsNullOrEmpty(Config.SelectedHueLightId))
                    {
                        await _hueService.SetColor(color, Config.SelectedHueLightId);
                    }

                    if (Config.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LIFXApiKey))
                    {
                        await _lifxService.SetColor(color, (Selector)Config.SelectedLIFXItemId);
                    }
                }
                catch (Exception ex)
                {
                    DiagnosticsClient.TrackException(ex);
                }
            }
        }


        private async void SetColor_Click(object sender, RoutedEventArgs e)
        {
            if (ColorGrid.SelectedColor.HasValue)
            {
                SignOutButton_Click(null, null);

                stopGraphPolling = true;
                stopThemePolling = true;
                string color = $"#{ColorGrid.HexadecimalString.ToString().Substring(3)}";

                if (!string.IsNullOrEmpty(Config.HueApiKey) && !string.IsNullOrEmpty(Config.HueIpAddress) && !string.IsNullOrEmpty(Config.SelectedHueLightId))
                {
                    await _hueService.SetColor(color, Config.SelectedHueLightId);
                }

                if (Config.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LIFXApiKey))
                {

                    await _lifxService.SetColor(color, (Selector)Config.SelectedLIFXItemId);
                }
            }
        }

    }
}

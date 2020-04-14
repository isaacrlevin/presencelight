using Microsoft.Graph;
using Microsoft.Identity.Client;
using PresenceLight.Core;
using PresenceLight.Core.Graph;
using System.Windows;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using Media = System.Windows.Media;
using System.Diagnostics;
using System.Windows.Navigation;
using PresenceLight.Core.Helpers;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;
using LifxCloud.NET.Models;
using System.Windows.Input;
namespace PresenceLight
{
    public partial class MainWindow : Window
    {
        private async void SyncTheme_Click(object sender, RoutedEventArgs e)
        {
            SignOutButton_Click(null, null);

            var theme = ((SolidColorBrush)SystemParameters.WindowGlassBrush).Color;
            stopGraphPolling = true;

            string color = $"#{theme.ToString().Substring(3)}";

            lblTheme.Content = $"Theme Color is {color}";
            lblTheme.Foreground = (SolidColorBrush)SystemParameters.WindowGlassBrush;
            lblTheme.Visibility = Visibility.Visible;

            if (!string.IsNullOrEmpty(Config.HueApiKey) && !string.IsNullOrEmpty(Config.HueIpAddress) && !string.IsNullOrEmpty(Config.SelectedHueLightId))
            {
                await _hueService.SetColor(color, Config.SelectedHueLightId);
            }

            if (Config.IsLifxEnabled && !string.IsNullOrEmpty(Config.LifxApiKey))
            {
                await _lifxService.SetColor(color, (Selector)Config.SelectedLifxItemId);
            }

            while (true)
            {
                if (stopThemePolling)
                {
                    stopThemePolling = false;
                    return;
                }
                await Task.Delay(5000);
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

                    if (Config.IsLifxEnabled && !string.IsNullOrEmpty(Config.LifxApiKey))
                    {
                        await _lifxService.SetColor(color, (Selector)Config.SelectedLifxItemId);
                    }
                }
                catch { }
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

                if (Config.IsLifxEnabled && !string.IsNullOrEmpty(Config.LifxApiKey))
                {

                    await _lifxService.SetColor(color, (Selector)Config.SelectedLifxItemId);
                }
            }
        }

    }
}

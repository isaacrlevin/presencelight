using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

using PresenceLight.Core;
using PresenceLight.Graph;

namespace PresenceLight
{
    public partial class MainWindow : Window
    {
        private async Task LoadSettings()
        {
            try
            {
                if (!(await _settingsService.IsFilePresent().ConfigureAwait(true)))
                {
                    await _settingsService.SaveSettings(_options).ConfigureAwait(true);
                }

                Config = await _settingsService.LoadSettings().ConfigureAwait(true) ?? throw new NullReferenceException("Settings Load Service Returned null");

                if (_workingHoursService.UseWorkingHours)
                {
                    pnlWorkingHours.Visibility = Visibility.Visible;
                    SyncOptions();
                }
                else
                {
                    pnlWorkingHours.Visibility = Visibility.Collapsed;
                    SyncOptions();
                }

                if (Config.LightSettings.Hue.IsEnabled)
                {
                    pnlPhillips.Visibility = Visibility.Visible;
                    pnlHueApi.Visibility = Visibility.Visible;
                    SyncOptions();
                }
                else
                {
                    pnlPhillips.Visibility = Visibility.Collapsed;
                    pnlHueApi.Visibility = Visibility.Collapsed;
                }

                if (Config.LightSettings.Yeelight.IsEnabled)
                {
                    pnlYeelight.Visibility = Visibility.Visible;
                    SyncOptions();
                }
                else
                {
                    pnlYeelight.Visibility = Visibility.Collapsed;
                }

                if (Config.LightSettings.LIFX.IsEnabled)
                {
                    getTokenLink.Visibility = Visibility.Visible;
                    pnlLIFX.Visibility = Visibility.Visible;

                    SyncOptions();
                }
                else
                {
                    getTokenLink.Visibility = Visibility.Collapsed;
                    pnlLIFX.Visibility = Visibility.Collapsed;
                }

                if (Config.LightSettings.CustomApi.IsEnabled)
                {
                    pnlCustomApi.Visibility = Visibility.Visible;

                    SyncOptions();
                }
                else
                {
                    pnlCustomApi.Visibility = Visibility.Collapsed;
                }
                if (!string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXClientId) && !(string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXClientSecret)))
                {
                    getTokenLink.Visibility = Visibility.Visible;
                }
                else
                {
                    getTokenLink.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occured Loading Settings");
                _diagClient.TrackException(e);
            }
        }

        private async void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnSettings.IsEnabled = false;
                if (Transparent.IsChecked == true)
                {
                    Config.IconType = "Transparent";
                }
                else
                {
                    Config.IconType = "White";
                }

                if (HourStatusKeep.IsChecked == true)
                {
                    Config.LightSettings.HoursPassedStatus = "Keep";
                }

                if (HourStatusOff.IsChecked == true)
                {
                    Config.LightSettings.HoursPassedStatus = "Off";
                }

                if (HourStatusWhite.IsChecked == true)
                {
                    Config.LightSettings.HoursPassedStatus = "White";
                }

                CheckAAD();
                Config.LightSettings.DefaultBrightness = Convert.ToInt32(brightness.Value);

                SetWorkingDays();

                SyncOptions();
                await _settingsService.SaveSettings(Config).ConfigureAwait(true);
                lblSettingSaved.Visibility = Visibility.Visible;
                btnSettings.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured Saving Settings");
                _diagClient.TrackException(ex);
            }
        }

        private void SetWorkingDays()
        {
            List<string> days = new List<string>();

            if (Monday.IsChecked != null && Monday.IsChecked.Value)
            {
                days.Add("Monday");
            }

            if (Tuesday.IsChecked != null && Tuesday.IsChecked.Value)
            {
                days.Add("Tuesday");
            }

            if (Wednesday.IsChecked != null && Wednesday.IsChecked.Value)
            {
                days.Add("Wednesday");
            }

            if (Thursday.IsChecked != null && Thursday.IsChecked.Value)
            {
                days.Add("Thursday");
            }

            if (Friday.IsChecked != null && Friday.IsChecked.Value)
            {
                days.Add("Friday");
            }

            if (Saturday.IsChecked != null && Saturday.IsChecked.Value)
            {
                days.Add("Saturday");
            }

            if (Sunday.IsChecked != null && Sunday.IsChecked.Value)
            {
                days.Add("Sunday");
            }

            Config.LightSettings.WorkingDays = string.Join("|", days);
        }

        private void CheckAAD()
        {
            try
            {
                SyncOptions();

                configErrorPanel.Visibility = Visibility.Hidden;

                if (dataPanel.Visibility != Visibility.Visible)
                {
                    signInPanel.Visibility = Visibility.Visible;
                }

                if (!_graphServiceClient.IsInitialized)
                {
                    _graphServiceClient.Initialize(_graphservice.GetAuthenticatedGraphClient());
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occured Checking Azure Active Directory");
                _diagClient.TrackException(e);
            }
        }

        private void PopulateWorkingDays()
        {
            if (!string.IsNullOrEmpty(Config.LightSettings.WorkingDays))
            {
                if (Config.LightSettings.WorkingDays.Contains("Monday", StringComparison.OrdinalIgnoreCase))
                {
                    Monday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Tuesday", StringComparison.OrdinalIgnoreCase))
                {
                    Tuesday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Wednesday", StringComparison.OrdinalIgnoreCase))
                {
                    Wednesday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Thursday", StringComparison.OrdinalIgnoreCase))
                {
                    Thursday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Friday", StringComparison.OrdinalIgnoreCase))
                {
                    Friday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Saturday", StringComparison.OrdinalIgnoreCase))
                {
                    Saturday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Sunday", StringComparison.OrdinalIgnoreCase))
                {
                    Sunday.IsChecked = true;
                }
            }
        }

        private async void cbSyncLights(object sender, RoutedEventArgs e)
        {
            if (!Config.LightSettings.SyncLights)
            {
                await SetColor("Off").ConfigureAwait(true);
                turnOffButton.Visibility = Visibility.Collapsed;
                turnOnButton.Visibility = Visibility.Visible;
            }

            SyncOptions();
            await _settingsService.SaveSettings(Config).ConfigureAwait(true);
            e.Handled = true;
        }

        private async void cbUseDefaultBrightnessChanged(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.UseDefaultBrightness)
            {
                pnlDefaultBrightness.Visibility = Visibility.Visible;
            }
            else
            {
                pnlDefaultBrightness.Visibility = Visibility.Collapsed;
            }

            SyncOptions();
            await _settingsService.SaveSettings(Config).ConfigureAwait(true);
            e.Handled = true;
        }

        private void cbUseWorkingHoursChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Config.LightSettings.WorkingHoursStartTime))
            {
                Config.LightSettings.WorkingHoursStartTime = Config.LightSettings.WorkingHoursStartTimeAsDate.HasValue ? Config.LightSettings.WorkingHoursStartTimeAsDate.Value.TimeOfDay.ToString() : string.Empty;
            }

            if (!string.IsNullOrEmpty(Config.LightSettings.WorkingHoursEndTime))
            {
                Config.LightSettings.WorkingHoursEndTime = Config.LightSettings.WorkingHoursEndTimeAsDate.HasValue ? Config.LightSettings.WorkingHoursEndTimeAsDate.Value.TimeOfDay.ToString() : string.Empty;
            }

            if (_workingHoursService.UseWorkingHours)
            {
                pnlWorkingHours.Visibility = Visibility.Visible;
            }
            else
            {
                pnlWorkingHours.Visibility = Visibility.Collapsed;
            }

            SyncOptions();
            e.Handled = true;
        }

        private void time_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (Config.LightSettings.WorkingHoursStartTimeAsDate.HasValue)
            {
                Config.LightSettings.WorkingHoursStartTime = Config.LightSettings.WorkingHoursStartTimeAsDate.HasValue ? Config.LightSettings.WorkingHoursStartTimeAsDate.Value.TimeOfDay.ToString() : string.Empty;
            }

            if (Config.LightSettings.WorkingHoursEndTimeAsDate.HasValue)
            {
                Config.LightSettings.WorkingHoursEndTime = Config.LightSettings.WorkingHoursEndTimeAsDate.HasValue ? Config.LightSettings.WorkingHoursEndTimeAsDate.Value.TimeOfDay.ToString() : string.Empty;
            }

            SyncOptions();
            e.Handled = true;
        }
    }
}

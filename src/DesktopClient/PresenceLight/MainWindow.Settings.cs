﻿using System;
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

                bool useWorkingHours = await _mediator.Send(new Core.WorkingHoursServices.UseWorkingHoursCommand());
                bool IsInWorkingHours = await _mediator.Send(new Core.WorkingHoursServices.IsInWorkingHoursCommand());

                if (useWorkingHours)
                {
                    settings.pnlWorkingHours.Visibility = Visibility.Visible;
                    SyncOptions();
                }
                else
                {
                    settings.pnlWorkingHours.Visibility = Visibility.Collapsed;
                    SyncOptions();
                }

                if (Config.LightSettings.Hue.IsEnabled)
                {
                    phillipsHue.pnlPhillips.Visibility = Visibility.Visible;
                    phillipsHue.pnlHueApi.Visibility = Visibility.Visible;
                    SyncOptions();
                }
                else
                {
                    phillipsHue.pnlPhillips.Visibility = Visibility.Collapsed;
                    phillipsHue.pnlHueApi.Visibility = Visibility.Collapsed;
                }

                if (Config.LightSettings.Yeelight.IsEnabled)
                {
                    yeelight.pnlYeelight.Visibility = Visibility.Visible;
                    SyncOptions();
                }
                else
                {
                    yeelight.pnlYeelight.Visibility = Visibility.Collapsed;
                }

                if (Config.LightSettings.LIFX.IsEnabled)
                {
                    if (!string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXClientId) && !(string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXClientSecret)))
                    {
                        lifx.getTokenLink.Visibility = Visibility.Visible;
                        lifx.pnlLIFX.Visibility = Visibility.Visible;
                    }
                    SyncOptions();
                }
                else
                {
                    lifx.getTokenLink.Visibility = Visibility.Collapsed;
                    lifx.pnlLIFX.Visibility = Visibility.Collapsed;
                }

                if (Config.LightSettings.CustomApi.IsEnabled)
                {
                    customapi.pnlCustomApi.Visibility = Visibility.Visible;

                    SyncOptions();
                }
                else
                {
                    customapi.pnlCustomApi.Visibility = Visibility.Collapsed;
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
                settings.btnSettings.IsEnabled = false;
                if (settings.Transparent.IsChecked == true)
                {
                    Config.IconType = "Transparent";
                }
                else
                {
                    Config.IconType = "White";
                }

                if (settings.HourStatusKeep.IsChecked == true)
                {
                    Config.LightSettings.HoursPassedStatus = "Keep";
                }

                if (settings.HourStatusOff.IsChecked == true)
                {
                    Config.LightSettings.HoursPassedStatus = "Off";
                }

                if (settings.HourStatusWhite.IsChecked == true)
                {
                    Config.LightSettings.HoursPassedStatus = "White";
                }

                CheckAAD();
                Config.LightSettings.DefaultBrightness = Convert.ToInt32(settings.brightness.Value);

                SetWorkingDays();

                SyncOptions();
                await _settingsService.SaveSettings(Config).ConfigureAwait(true);
                settings.lblSettingSaved.Visibility = Visibility.Visible;
                settings.btnSettings.IsEnabled = true;
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

            if (settings.Monday.IsChecked != null && settings.Monday.IsChecked.Value)
            {
                days.Add("Monday");
            }

            if (settings.Tuesday.IsChecked != null && settings.Tuesday.IsChecked.Value)
            {
                days.Add("Tuesday");
            }

            if (settings.Wednesday.IsChecked != null && settings.Wednesday.IsChecked.Value)
            {
                days.Add("Wednesday");
            }

            if (settings.Thursday.IsChecked != null && settings.Thursday.IsChecked.Value)
            {
                days.Add("Thursday");
            }

            if (settings.Friday.IsChecked != null && settings.Friday.IsChecked.Value)
            {
                days.Add("Friday");
            }

            if (settings.Saturday.IsChecked != null && settings.Saturday.IsChecked.Value)
            {
                days.Add("Saturday");
            }

            if (settings.Sunday.IsChecked != null && settings.Sunday.IsChecked.Value)
            {
                days.Add("Sunday");
            }

            Config.LightSettings.WorkingDays = string.Join("|", days);
        }

        private async void CheckAAD()
        {
            try
            {
                SyncOptions();

                landingPage.configErrorPanel.Visibility = Visibility.Hidden;

                if (landingPage.dataPanel.Visibility != Visibility.Visible)
                {
                    landingPage.signInPanel.Visibility = Visibility.Visible;
                }

                if (!await _mediator.Send(new Core.GraphServices.GetIsInitializedCommand()))
                {
                    await _mediator.Send(new Core.GraphServices.InitializeCommand()
                    {
                        Client = _graphservice.GetAuthenticatedGraphClient()
                    });

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
                    settings.Monday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Tuesday", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Tuesday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Wednesday", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Wednesday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Thursday", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Thursday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Friday", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Friday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Saturday", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Saturday.IsChecked = true;
                }

                if (Config.LightSettings.WorkingDays.Contains("Sunday", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Sunday.IsChecked = true;
                }
            }
        }

        private async void cbSyncLights(object sender, RoutedEventArgs e)
        {
            if (!Config.LightSettings.SyncLights)
            {
                await SetColor("Off").ConfigureAwait(true);
                landingPage.turnOffButton.Visibility = Visibility.Collapsed;
                landingPage.turnOnButton.Visibility = Visibility.Visible;
            }

            SyncOptions();
            await _settingsService.SaveSettings(Config).ConfigureAwait(true);
            e.Handled = true;
        }

        private async void cbUseDefaultBrightnessChanged(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.UseDefaultBrightness)
            {
                settings.pnlDefaultBrightness.Visibility = Visibility.Visible;
            }
            else
            {
                settings.pnlDefaultBrightness.Visibility = Visibility.Collapsed;
            }

            SyncOptions();
            await _settingsService.SaveSettings(Config).ConfigureAwait(true);
            e.Handled = true;
        }

        private async void cbUseWorkingHoursChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Config.LightSettings.WorkingHoursStartTime))
            {
                Config.LightSettings.WorkingHoursStartTime = Config.LightSettings.WorkingHoursStartTimeAsDate.HasValue ? Config.LightSettings.WorkingHoursStartTimeAsDate.Value.TimeOfDay.ToString() : string.Empty;
            }

            if (!string.IsNullOrEmpty(Config.LightSettings.WorkingHoursEndTime))
            {
                Config.LightSettings.WorkingHoursEndTime = Config.LightSettings.WorkingHoursEndTimeAsDate.HasValue ? Config.LightSettings.WorkingHoursEndTimeAsDate.Value.TimeOfDay.ToString() : string.Empty;
            }
            bool useWorkingHours = await _mediator.Send(new Core.WorkingHoursServices.UseWorkingHoursCommand());

            if (useWorkingHours)
            {
                settings.pnlWorkingHours.Visibility = Visibility.Visible;
            }
            else
            {
                settings.pnlWorkingHours.Visibility = Visibility.Collapsed;
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

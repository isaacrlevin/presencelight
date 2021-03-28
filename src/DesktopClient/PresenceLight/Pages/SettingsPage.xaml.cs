using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PresenceLight.Services;
using PresenceLight.Telemetry;
using ModernWpf;
using PresenceLight.Core;
using System.Windows.Controls;
using PresenceLight.Core.Configuration;
using LogLevel = PresenceLight.Core.Configuration.LogLevel;

namespace PresenceLight.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage
    {
        private MainWindowModern _parentWindow;
        private DiagnosticsClient _diagClient;
        private ILogger _logger;

        public SettingsPage()
        {
            _diagClient = App.ServiceProvider.GetRequiredService<DiagnosticsClient>();
            _logger = App.ServiceProvider.GetRequiredService<ILogger<SettingsPage>>();
            _parentWindow = Application.Current.Windows.OfType<MainWindowModern>().First();
            InitializeComponent();

            ddlLogLevel.ItemsSource = LogLevel.GetAll();



            foreach (var item in ddlLogLevel.Items)
            {
                var log = (LogLevel)item;
                if (log.Label == SettingsHandlerBase.Config.LogLevel)
                {
                    ddlLogLevel.SelectedItem = item;
                }
            }


            if (SettingsHandlerBase.Config.LightSettings.UseWorkingHours)
            {

                pnlWorkingHours.Visibility = Visibility.Visible;
            }
            else
            {
                pnlWorkingHours.Visibility = Visibility.Collapsed;
            }


            switch (SettingsHandlerBase.Config.Theme)
            {
                case "Light":
                    themeLight.IsChecked = true;
                    break;
                case "Dark":
                    themeDark.IsChecked = true;
                    break;
                case "Use system setting":
                    themeDefault.IsChecked = true;
                    break;
                default:
                    themeDefault.IsChecked = true;
                    break;
            }
            if (SettingsHandlerBase.Config.IconType == "Transparent")
            {
                Transparent.IsChecked = true;
            }
            else
            {
                White.IsChecked = true;
            }

            switch (SettingsHandlerBase.Config.LightSettings.HoursPassedStatus)
            {
                case "Keep":
                    HourStatusKeep.IsChecked = true;
                    break;
                case "White":
                    HourStatusWhite.IsChecked = true;
                    break;
                case "Off":
                    HourStatusOff.IsChecked = true;
                    break;
                default:
                    HourStatusKeep.IsChecked = true;
                    break;
            }
            if (SettingsHandlerBase.Config.IconType == "Transparent")
            {
                Transparent.IsChecked = true;
            }
            else
            {

                White.IsChecked = true;
            }

            PopulateWorkingDays();
        }

        private void OnThemeToggle(object sender, RoutedEventArgs e)
        {
            if (themeDark.IsChecked.Value)
            {
                SettingsHandlerBase.Config.Theme = "Dark";
            }

            if (themeDefault.IsChecked.Value)
            {
                SettingsHandlerBase.Config.Theme = "Use default setting";
            }

            if (themeLight.IsChecked.Value)
            {
                SettingsHandlerBase.Config.Theme = "Light";
            }

            ThemeManager.Current.ApplicationTheme = SettingsHandlerBase.Config.Theme switch
            {
                "Light" => ApplicationTheme.Light,
                "Dark" => ApplicationTheme.Dark,
                "Use system setting" => null,
                _ => null,
            };
        }
        private async void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnSettings.IsEnabled = false;

                foreach (var item in ddlLogLevel.ItemsSource)
                {
                    var log = (LogLevel)item;
                    if (log.Label == ((LogLevel)ddlLogLevel.SelectedItem).Label)
                    {
                        SettingsHandlerBase.Config.LogLevel = log.Label;
                    }
                }

                if (Transparent.IsChecked == true)
                {
                    SettingsHandlerBase.Config.IconType = "Transparent";
                }
                else
                {
                    SettingsHandlerBase.Config.IconType = "White";
                }

                if (HourStatusKeep.IsChecked == true)
                {
                    SettingsHandlerBase.Config.LightSettings.HoursPassedStatus = "Keep";
                }

                if (HourStatusOff.IsChecked == true)
                {
                    SettingsHandlerBase.Config.LightSettings.HoursPassedStatus = "Off";
                }

                if (HourStatusWhite.IsChecked == true)
                {
                    SettingsHandlerBase.Config.LightSettings.HoursPassedStatus = "White";
                }
                SettingsHandlerBase.Config.LightSettings.DefaultBrightness = Convert.ToInt32(brightness.Value);

                CheckAAD();


                SetWorkingDays();

                SettingsHandlerBase.SyncOptions();
                if (!await _parentWindow._mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true))
                {
                    _logger.LogDebug(SettingsHandlerBase.Config, "Settings Not Saved Properly");
                }

                lblSettingSaved.Visibility = Visibility.Visible;
                btnSettings.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(SettingsHandlerBase.Config, ex, "Error occured Saving Settings");
                _diagClient.TrackException(ex);

            }
        }

        private void SetWorkingDays()
        {
            var days = new List<string>();

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

            SettingsHandlerBase.Config.LightSettings.WorkingDays = string.Join("|", days);
        }

        private async void CheckAAD()
        {
            try
            {
                SettingsHandlerBase.SyncOptions();

                if (!await _parentWindow._mediator.Send(new Core.GraphServices.GetIsInitializedCommand()))
                {
                    await _parentWindow._mediator.Send(new Core.GraphServices.InitializeCommand()
                    {
                        Client = _parentWindow._graphservice.GetAuthenticatedGraphClient()
                    });

                }
            }
            catch (Exception e)
            {
                _logger.LogError(SettingsHandlerBase.Config, e, "Error occured Checking Azure Active Directory");
                _diagClient.TrackException(e);
            }
        }

        private void PopulateWorkingDays()
        {
            if (!string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.WorkingDays))
            {
                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Monday", StringComparison.OrdinalIgnoreCase))
                {
                    Monday.IsChecked = true;
                }

                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Tuesday", StringComparison.OrdinalIgnoreCase))
                {
                    Tuesday.IsChecked = true;
                }

                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Wednesday", StringComparison.OrdinalIgnoreCase))
                {
                    Wednesday.IsChecked = true;
                }

                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Thursday", StringComparison.OrdinalIgnoreCase))
                {
                    Thursday.IsChecked = true;
                }

                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Friday", StringComparison.OrdinalIgnoreCase))
                {
                    Friday.IsChecked = true;
                }

                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Saturday", StringComparison.OrdinalIgnoreCase))
                {
                    Saturday.IsChecked = true;
                }

                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Sunday", StringComparison.OrdinalIgnoreCase))
                {
                    Sunday.IsChecked = true;
                }
            }
        }

        private async void cbSyncLights(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private async void cbUseDefaultBrightnessChanged(object sender, RoutedEventArgs e)
        {
            if (SettingsHandlerBase.Config.LightSettings.UseDefaultBrightness)
            {
                pnlDefaultBrightness.Visibility = Visibility.Visible;
            }
            else
            {
                pnlDefaultBrightness.Visibility = Visibility.Collapsed;
            }

            SettingsHandlerBase.SyncOptions();
            if (!await _parentWindow._mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true))
            {
                _logger.LogDebug(SettingsHandlerBase.Config, "Settings Not Saved Properly");
            }
            e.Handled = true;
        }

        private async void cbUseWorkingHoursChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTime))
            {
                SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTime = SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTimeAsDate.HasValue ? SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTimeAsDate.Value.TimeOfDay.ToString() : string.Empty;
            }

            if (!string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTime))
            {
                SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTime = SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTimeAsDate.HasValue ? SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTimeAsDate.Value.TimeOfDay.ToString() : string.Empty;
            }
            var useWorkingHours = await _parentWindow._mediator.Send(new Core.WorkingHoursServices.UseWorkingHoursCommand());

            if (useWorkingHours)
            {
                pnlWorkingHours.Visibility = Visibility.Visible;
            }
            else
            {
                pnlWorkingHours.Visibility = Visibility.Collapsed;
            }

            SettingsHandlerBase.SyncOptions();
            e.Handled = true;
        }

        private void time_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTimeAsDate.HasValue)
            {
                SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTime = SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTimeAsDate.HasValue ? SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTimeAsDate.Value.TimeOfDay.ToString() : string.Empty;
            }

            if (SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTimeAsDate.HasValue)
            {
                SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTime = SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTimeAsDate.HasValue ? SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTimeAsDate.Value.TimeOfDay.ToString() : string.Empty;
            }

            SettingsHandlerBase.SyncOptions();
            e.Handled = true;
        }

        private void ddlLogLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
     
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PresenceLight.Core;
using PresenceLight.Services;
using PresenceLight.Telemetry;

namespace PresenceLight.Pages
{
    /// <summary>
    /// Interaction logic for CustomApiPage.xaml
    /// </summary>
    [ContentProperty("Content")]
    public partial class CustomApiPage : Page
    {
        private MediatR.IMediator _mediator;

        ILogger _logger;
        public CustomApiPage()
        {
            _mediator = App.Host.Services.GetRequiredService<MediatR.IMediator>();

            _logger = App.Host.Services.GetRequiredService<ILogger<CustomApiPage>>();

            InitializeComponent();
            if (SettingsHandlerBase.Config.LightSettings.CustomApi.IsEnabled)
            {
                pnlCustomApi.Visibility = Visibility.Visible;

                SettingsHandlerBase.SyncOptions();
            }
            else
            {
                pnlCustomApi.Visibility = Visibility.Collapsed;
            }
        }

        private async void btnApiSettingsSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                await _mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);

                lblCustomApiSaved.Visibility = Visibility.Visible;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured Saving Custom Api Settings");
                //TODO: Revisit if Telemtry isnt working through serilog
                //_diagClient.TrackException(ex);
            }
        }
        private void cbIsCustomApiEnabledChanged(object sender, RoutedEventArgs e)
        {
             
            pnlCustomApi.Visibility = (SettingsHandlerBase.Config.LightSettings.CustomApi.IsEnabled = (sender as CheckBox)?.IsChecked ?? false)
                ? Visibility.Visible
                : Visibility.Collapsed;

            SettingsHandlerBase.SyncOptions();

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
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiAvailable.Method = selectedText;
                    break;
                case "customApiBusyMethod":
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiBusy.Method = selectedText;
                    break;
                case "customApiBeRightBackMethod":
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiBeRightBack.Method = selectedText;
                    break;
                case "customApiAwayMethod":
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiAway.Method = selectedText;
                    break;
                case "customApiDoNotDisturbMethod":
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiDoNotDisturb.Method = selectedText;
                    break;
                case "customApiAvailableIdleMethod":
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiAvailableIdle.Method = selectedText;
                    break;
                case "customApiOfflineMethod":
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiOffline.Method = selectedText;
                    break;
                case "customApiOffMethod":
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiOff.Method = selectedText;
                    break;
                case "customApiActivityAvailableMethod":
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiActivityAvailable.Method = selectedText;
                    break;
                case "customApiActivityPresentingMethod":
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiActivityPresenting.Method = selectedText;
                    break;
                case "customApiActivityInACallMethod":
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiActivityInACall.Method = selectedText;
                    break;
                case "customApiActivityInAMeetingMethod":
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiActivityInAMeeting.Method = selectedText;
                    break;
                case "customApiActivityBusyMethod":
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiActivityBusy.Method = selectedText;
                    break;
                case "customApiActivityAwayMethod":
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiActivityAway.Method = selectedText;
                    break;
                case "customApiActivityBeRightBackMethod":
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiActivityBeRightBack.Method = selectedText;
                    break;
                case "customApiActivityDoNotDisturbMethod":
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiActivityDoNotDisturb.Method = selectedText;
                    break;
                case "customApiActivityIdleMethod":
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiActivityIdle.Method = selectedText;
                    break;
                case "customApiActivityOfflineMethod":
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiActivityOffline.Method = selectedText;
                    break;
                case "customApiActivityOffMethod":
                    SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiActivityOff.Method = selectedText;
                    break;
                default:
                    break;
            }

            e.Handled = true;
        }
    }
}

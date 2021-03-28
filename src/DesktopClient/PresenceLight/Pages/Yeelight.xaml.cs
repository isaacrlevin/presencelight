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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PresenceLight.Services;

namespace PresenceLight.Pages
{
    /// <summary>
    /// Interaction logic for xaml
    /// </summary>
    public partial class Yeelight : Page
    {
        private MediatR.IMediator _mediator;

        ILogger _logger;
        public Yeelight()
        {
            _mediator = App.ServiceProvider.GetRequiredService<MediatR.IMediator>();

            _logger = App.ServiceProvider.GetRequiredService<ILogger<Yeelight>>();

             InitializeComponent();
            pnlYeelight.Visibility = SettingsHandlerBase.Config.LightSettings.Yeelight.IsEnabled ? Visibility.Visible : Visibility.Collapsed;

            if (SettingsHandlerBase.Config.LightSettings.UseDefaultBrightness)
            {
                yeelightBrightness.IsEnabled = false;
                yeelightBrightnessNum.IsEnabled = false;
                yeelightBrightnessText.Visibility = Visibility.Visible;
            }
            else
            {
                yeelightBrightness.IsEnabled = true;
                yeelightBrightnessNum.IsEnabled = true;
                yeelightBrightnessText.Visibility = Visibility.Collapsed;
            }

            CheckYeelight();
        }

        #region Yeelight Panel

        private void cbIsYeelightEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (SettingsHandlerBase.Config.LightSettings.Yeelight.IsEnabled)
            {
                pnlYeelight.Visibility = Visibility.Visible;
            }
            else
            {
                pnlYeelight.Visibility = Visibility.Collapsed;
            }

            SettingsHandlerBase.SyncOptions();
            e.Handled = true;
        }

        private async void FindYeelights_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pnlYeelightData.Visibility = Visibility.Collapsed;

                var deviceGroup = await _mediator.Send(new Core.YeelightServices.FindLightsCommand()).ConfigureAwait(true);

                ddlYeelightLights.ItemsSource = deviceGroup.ToList();
                pnlYeelightData.Visibility = Visibility.Visible;

                if (SettingsHandlerBase.Config.LightSettings.Yeelight.UseActivityStatus)
                {
                    pnlYeelightActivityStatuses.Visibility = Visibility.Visible;
                    pnlYeelightAvailableStatuses.Visibility = Visibility.Collapsed;
                }
                else
                {
                    pnlYeelightAvailableStatuses.Visibility = Visibility.Visible;
                    pnlYeelightActivityStatuses.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(SettingsHandlerBase.Config, ex, "Error occured Finding YeeLights");
                //TODO: Come back to this if necessary
                //_diagClient.TrackException(ex);
            }
        }

        private void ddlYeelightLights_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ddlYeelightLights.SelectedItem != null)
            {
                SettingsHandlerBase.Config.LightSettings.Yeelight.SelectedItemId = ((YeelightAPI.Device)ddlYeelightLights.SelectedItem).Id;
                SettingsHandlerBase.SyncOptions();
            }
            e.Handled = true;
        }

        private async void CheckYeelight()
        {
            try
            {
                
                pnlYeelightData.Visibility = Visibility.Collapsed;
                if (SettingsHandlerBase.Config != null)
                {
                    SettingsHandlerBase.SyncOptions();

                    ddlYeelightLights.ItemsSource = await _mediator.Send(new Core.YeelightServices.FindLightsCommand()).ConfigureAwait(true);

                    foreach (var item in ddlYeelightLights.Items)
                    {
                        var light = (YeelightAPI.Device)item;
                        if (light?.Id == SettingsHandlerBase.Config.LightSettings.Yeelight.SelectedItemId)
                        {
                            ddlYeelightLights.SelectedItem = item;
                        }
                    }
                    ddlYeelightLights.Visibility = Visibility.Visible;
                    pnlYeelightData.Visibility = Visibility.Visible;

                    if (SettingsHandlerBase.Config.LightSettings.Yeelight.UseActivityStatus)
                    {
                        pnlYeelightActivityStatuses.Visibility = Visibility.Visible;
                        pnlYeelightAvailableStatuses.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        pnlYeelightAvailableStatuses.Visibility = Visibility.Visible;
                        pnlYeelightActivityStatuses.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(SettingsHandlerBase.Config, e, "Error occured Checking YeeLight");

                //TODO:  Come back to this if necessary
                //_diagClient.TrackException(e);
            }
        }

        private void cbUseYeelightActivityStatus(object sender, RoutedEventArgs e)
        {
            if (SettingsHandlerBase.Config.LightSettings.Yeelight.UseActivityStatus)
            {
                pnlYeelightAvailableStatuses.Visibility = Visibility.Collapsed;
                pnlYeelightActivityStatuses.Visibility = Visibility.Visible;
            }
            else
            {
                pnlYeelightAvailableStatuses.Visibility = Visibility.Visible;
                pnlYeelightActivityStatuses.Visibility = Visibility.Collapsed;
            }
            SettingsHandlerBase.SyncOptions();
            e.Handled = true;
        }

        private void cbYeelightIsDisabledChange(object sender, RoutedEventArgs e)
        {
            CheckBox cb = e.Source as CheckBox ?? throw new ArgumentException("Check Box Not Found");
            var cbName = cb.Name.Replace("Disabled", "Colour");
            var colorpicker = (Xceed.Wpf.Toolkit.ColorPicker)this.FindName(cbName);
            colorpicker.IsEnabled = cb.IsChecked != true;
            SettingsHandlerBase.SyncOptions();
            e.Handled = true;
        }

        private async void SaveYeelight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnYeelight.IsEnabled = false;
                await _mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);
                
                
                CheckYeelight();
                lblYeelightSaved.Visibility = Visibility.Visible;
                btnYeelight.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(SettingsHandlerBase.Config, ex, "Error Occured Saving Yeelight Settings");
                //TODO : COME BACK IF NECESSARY
                //_diagClient.TrackException(ex);
            }
        }
        #endregion
    }
}

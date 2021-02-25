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

using PresenceLight.Core;
using PresenceLight.Core.WizServices;
using PresenceLight.Services;
using PresenceLight.Telemetry;

namespace PresenceLight.Controls
{
    /// <summary>
    /// Interaction logic for xaml
    /// </summary>
    public partial class Wiz : UserControl
    {
        private MainWindowModern parentWindow;
        private DiagnosticsClient _diagClient;
        private MediatR.IMediator _mediator;

        ILogger _logger;
        public Wiz()
        {
            _mediator = App.Host.Services.GetRequiredService<MediatR.IMediator>();
            _diagClient = App.Host.Services.GetRequiredService<DiagnosticsClient>();
            _logger = App.Host.Services.GetRequiredService<ILogger<Wiz>>();

            parentWindow = System.Windows.Application.Current.Windows.OfType<MainWindowModern>().First();

            InitializeComponent();
        }
   
        private void cbIsWizEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (SettingsHandlerBase.Config.LightSettings.Wiz.IsEnabled)
            {
                pnlWiz.Visibility = Visibility.Visible;
            }
            else
            {
                pnlWiz.Visibility = Visibility.Collapsed;
            }
            SettingsHandlerBase.SyncOptions();
            e.Handled = true;
        }
        #region Wiz Panel

 
        private async void FindWizs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pnlWizData.Visibility = Visibility.Collapsed;
                var lightList = await _mediator.Send(new Core.WizServices.GetLightsCommand()).ConfigureAwait(true);

                ddlWizLights.ItemsSource = lightList;
                pnlWizData.Visibility = Visibility.Visible;

                if (SettingsHandlerBase.Config.LightSettings.Wiz.UseActivityStatus)
                {
                    pnlWizActivityStatuses.Visibility = Visibility.Visible;
                    pnlWizAvailableStatuses.Visibility = Visibility.Collapsed;
                }
                else
                {
                    pnlWizAvailableStatuses.Visibility = Visibility.Visible;
                    pnlWizActivityStatuses.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured Finding Wiz");
                _diagClient.TrackException(ex);
            }
        }

        private void ddlWizLights_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ddlWizLights.SelectedItem != null)
            {
                var selectedWizItem = (WizLight)ddlWizLights.SelectedItem;
                SettingsHandlerBase.Config.LightSettings.Wiz.SelectedItemId = selectedWizItem.MacAddress;
                SettingsHandlerBase.SyncOptions();
            }
            e.Handled = true;
        }

        private async void CheckWiz()
        {
            try
            {
                pnlWizData.Visibility = Visibility.Collapsed;
                if (SettingsHandlerBase.Config != null)
                {
                    SettingsHandlerBase.SyncOptions();

                    ddlWizLights.ItemsSource = await _mediator.Send(new Core.WizServices.GetLightsCommand()).ConfigureAwait(true);


                    foreach (var item in ddlWizLights.Items)
                    {
                        var light = (WizLight)item;
                        if (light.MacAddress == SettingsHandlerBase.Config.LightSettings.Wiz.SelectedItemId)
                        {
                            ddlWizLights.SelectedItem = item;
                        }
                    }
                    ddlWizLights.Visibility = Visibility.Visible;
                    pnlWizData.Visibility = Visibility.Visible;

                    if (SettingsHandlerBase.Config.LightSettings.Wiz.UseActivityStatus)
                    {
                        pnlWizActivityStatuses.Visibility = Visibility.Visible;
                        pnlWizAvailableStatuses.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        pnlWizAvailableStatuses.Visibility = Visibility.Visible;
                        pnlWizActivityStatuses.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occured Checking Wiz");
                _diagClient.TrackException(e);
            }
        }

        private void cbUseWizActivityStatus(object sender, RoutedEventArgs e)
        {
            if (SettingsHandlerBase.Config.LightSettings.Wiz.UseActivityStatus)
            {
                pnlWizAvailableStatuses.Visibility = Visibility.Collapsed;
                pnlWizActivityStatuses.Visibility = Visibility.Visible;
            }
            else
            {
                pnlWizAvailableStatuses.Visibility = Visibility.Visible;
                pnlWizActivityStatuses.Visibility = Visibility.Collapsed;
            }
            SettingsHandlerBase.SyncOptions();
            e.Handled = true;
        }

        private void cbWizIsDisabledChange(object sender, RoutedEventArgs e)
        {
            CheckBox cb = e.Source as CheckBox ?? throw new ArgumentException("Check Box Not Found");
            var cbName = cb.Name.Replace("Disabled", "Colour");
            var colorpicker = (Xceed.Wpf.Toolkit.ColorPicker)this.FindName(cbName);
            colorpicker.IsEnabled = !cb.IsChecked.Value;
            SettingsHandlerBase.SyncOptions();
            e.Handled = true;
        }

        private async void SaveWiz_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnWiz.IsEnabled = false;
                await _mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);
                
                CheckWiz();
                lblWizSaved.Visibility = Visibility.Visible;
                btnWiz.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured Saving Wiz Settings");
                _diagClient.TrackException(ex);
            }
        }
        #endregion
    }
}

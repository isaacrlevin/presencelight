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
using PresenceLight.ViewModels;

namespace PresenceLight.Pages
{
    /// <summary>
    /// Interaction logic for xaml
    /// </summary>
    public partial class Wiz
    {
        private DiagnosticsClient _diagClient;
        private MediatR.IMediator _mediator;

        ILogger _logger;
        public Wiz()
        {
            DataContext = App.ServiceProvider.GetRequiredService<WizVm>();

            _mediator = App.ServiceProvider.GetRequiredService<MediatR.IMediator>();
            _diagClient = App.ServiceProvider.GetRequiredService<DiagnosticsClient>();
            _logger = App.ServiceProvider.GetRequiredService<ILogger<Wiz>>();

            InitializeComponent();
        }

        //private void cbIsWizEnabledChanged(object sender, RoutedEventArgs e)
        //{
        //    if (SettingsHandlerBase.Config.LightSettings.UseDefaultBrightness)
        //    {
        //        WizBrightness.IsEnabled = false;
        //        WizBrightnessNum.IsEnabled = false;
        //        wizBrightnessText.Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        WizBrightness.IsEnabled = true;
        //        WizBrightnessNum.IsEnabled = true;
        //        wizBrightnessText.Visibility = Visibility.Collapsed;
        //    }

        //    SettingsHandlerBase.SyncOptions();
        //    e.Handled = true;
        //}
        #region Wiz Panel

        //private void ddlWizLights_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        //{
        //    if (ddlWizLights.SelectedItem != null)
        //    {
        //        var selectedWizItem = (WizLight)ddlWizLights.SelectedItem;
        //        SettingsHandlerBase.Config.LightSettings.Wiz.SelectedItemId = selectedWizItem.MacAddress;
        //        SettingsHandlerBase.SyncOptions();
        //    }
        //    e.Handled = true;
        //}

        //private void cbUseWizActivityStatus(object sender, RoutedEventArgs e)
        //{
        //    if (SettingsHandlerBase.Config.LightSettings.Wiz.UseActivityStatus)
        //    {
        //        pnlWizAvailableStatuses.Visibility = Visibility.Collapsed;
        //        pnlWizActivityStatuses.Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        pnlWizAvailableStatuses.Visibility = Visibility.Visible;
        //        pnlWizActivityStatuses.Visibility = Visibility.Collapsed;
        //    }
        //    SettingsHandlerBase.SyncOptions();
        //    e.Handled = true;
        //}

        //private void cbWizIsDisabledChange(object sender, RoutedEventArgs e)
        //{
        //    CheckBox cb = e.Source as CheckBox ?? throw new ArgumentException("Check Box Not Found");
        //    var cbName = cb.Name.Replace("Disabled", "Colour");
        //    var colorpicker = (Xceed.Wpf.Toolkit.ColorPicker)this.FindName(cbName);
        //    colorpicker.IsEnabled = !cb.IsChecked.Value;
        //    SettingsHandlerBase.SyncOptions();
        //    e.Handled = true;
        //}

        //private async void SaveWiz_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        await _mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);

        //        CheckWiz();
        //        lblWizSaved.Visibility = Visibility.Visible;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error Occured Saving Wiz Settings");
        //        _diagClient.TrackException(ex);
        //    }
        //}
        #endregion
    }
}

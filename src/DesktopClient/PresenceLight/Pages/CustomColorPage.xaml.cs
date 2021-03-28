using System;
using System.Collections.Generic;
using System.Globalization;
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
using PresenceLight.Services;
using PresenceLight.Telemetry;

namespace PresenceLight.Pages
{

    public partial class CustomColorPage
    {
        private MediatR.IMediator _mediator;
        MainWindowModern parentWindow;
        private  DiagnosticsClient _diagClient;
        ILogger _logger;

        public CustomColorPage()
        {
            _mediator = App.ServiceProvider.GetRequiredService<MediatR.IMediator>();
            _diagClient = App.ServiceProvider.GetRequiredService<DiagnosticsClient>();
            _logger = App.ServiceProvider.GetRequiredService<ILogger<CustomColorPage>>();

         
            InitializeComponent();
            parentWindow = Application.Current.Windows.OfType<MainWindowModern>().First();
        }

        private void SetTeamsPresence_Click(object sender, RoutedEventArgs e)
        {
            parentWindow.LightMode = "Graph";
            syncTeamsButton.IsEnabled = false;
            syncThemeButton.IsEnabled = true;
        }

        private async void SyncTheme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                parentWindow.LightMode = "Theme";
                syncTeamsButton.IsEnabled = true;
                syncThemeButton.IsEnabled = false;

                var theme = ((SolidColorBrush)SystemParameters.WindowGlassBrush).Color;

                string color = $"#{theme.ToString(CultureInfo.InvariantCulture).Substring(3)}";

                lblTheme.Content = $"Theme Color is {color}";
                lblTheme.Foreground = (SolidColorBrush)SystemParameters.WindowGlassBrush;
                lblTheme.Visibility = Visibility.Visible;

             
                await _mediator.Publish(new SetColorNotification(color)).ConfigureAwait(true);

                _logger.LogInformation(SettingsHandlerBase.Config, color);
            }
            catch (Exception ex)
            {
                _logger.LogError(SettingsHandlerBase.Config, ex, "Error occured Setting Theme Color");
                _diagClient.TrackException(ex);
            }
        }

        private async void SetColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ColorGrid.SelectedColor.HasValue)
                {
                    parentWindow.LightMode = "Custom";
                    syncTeamsButton.IsEnabled = true;
                    syncThemeButton.IsEnabled = true;

                    string color = $"#{ColorGrid.HexadecimalString.ToString().Substring(3)}";

                    if (parentWindow.LightMode == "Custom")
                    {
                        await _mediator.Publish(new SetColorNotification(color)).ConfigureAwait(true);
                         
                    }

                    _logger.LogInformation(SettingsHandlerBase.Config, color);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(SettingsHandlerBase.Config, ex, "Error occured Setting Custom Color");
                _diagClient.TrackException(ex);
            }
        }

       
    }
}

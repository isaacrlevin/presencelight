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

namespace PresenceLight.Pages
{

    public partial class CustomColorPage
    {
        private MediatR.IMediator _mediator;
        MainWindowModern parentWindow;
        ILogger _logger;

        public CustomColorPage()
        {
            _mediator = App.Host.Services.GetRequiredService<MediatR.IMediator>();

            _logger = App.Host.Services.GetRequiredService<ILogger<CustomColorPage>>();

         
            InitializeComponent();
            parentWindow = Application.Current.Windows.OfType<MainWindowModern>().First();
        }

        private void SetTeamsPresence_Click(object sender, RoutedEventArgs e)
        {
            parentWindow.lightMode = "Graph";
            syncTeamsButton.IsEnabled = false;
            syncThemeButton.IsEnabled = true;
        }

        private async void SyncTheme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                parentWindow.lightMode = "Theme";
                syncTeamsButton.IsEnabled = true;
                syncThemeButton.IsEnabled = false;

                var theme = ((SolidColorBrush)SystemParameters.WindowGlassBrush).Color;

                string color = $"#{theme.ToString(CultureInfo.InvariantCulture).Substring(3)}";

                lblTheme.Content = $"Theme Color is {color}";
                lblTheme.Foreground = (SolidColorBrush)SystemParameters.WindowGlassBrush;
                lblTheme.Visibility = Visibility.Visible;

                //await SetColor(color).ConfigureAwait(true);

                parentWindow._logger.LogInformation(color);
            }
            catch (Exception ex)
            {
                parentWindow._logger.LogError(ex, "Error occured Setting Theme Color");
                parentWindow._diagClient.TrackException(ex);
            }
        }

        private async void SetColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ColorGrid.SelectedColor.HasValue)
                {
                    parentWindow.lightMode = "Custom";
                    syncTeamsButton.IsEnabled = true;
                    syncThemeButton.IsEnabled = true;

                    string color = $"#{ColorGrid.HexadecimalString.ToString().Substring(3)}";

                    if (parentWindow.lightMode == "Custom")
                    {
                        //SetColor(color).ConfigureAwait(true);
                    }

                    parentWindow._logger.LogInformation(color);
                }
            }
            catch (Exception ex)
            {
                parentWindow._logger.LogError(ex, "Error occured Setting Custom Color");
                parentWindow._diagClient.TrackException(ex);
            }
        }
    }
}

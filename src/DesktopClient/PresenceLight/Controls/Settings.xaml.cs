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

namespace PresenceLight.Controls
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
        }

        public event EventHandler<RoutedEventArgs>? oncbSyncLights;
        public event EventHandler<RoutedEventArgs>? oncbUseDefaultBrightnessChanged;
        public event EventHandler<RoutedEventArgs>? oncbUseWorkingHoursChanged;
        public event EventHandler<RoutedEventArgs>? onSaveSettingsClick;
        public event EventHandler<RoutedPropertyChangedEventArgs<object>>? ontimeValueChanged;




        private async void time_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var handler = ontimeValueChanged;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            var handler = onSaveSettingsClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void cbSyncLights(object sender, RoutedEventArgs e)
        {
            var handler = oncbSyncLights;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void cbUseDefaultBrightnessChanged(object sender, RoutedEventArgs e)
        {
            var handler = oncbUseDefaultBrightnessChanged;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void cbUseWorkingHoursChanged(object sender, RoutedEventArgs e)
        {
            var handler = oncbUseWorkingHoursChanged;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }

    }
}

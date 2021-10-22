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
    /// Interaction logic for CustomApi.xaml
    /// </summary>
    public partial class Mqtt : UserControl
    {
        public Mqtt()
        {
            InitializeComponent();
        }
        public event EventHandler<RoutedEventArgs>? onbtnMqttSettingsSaveClick;
        public event EventHandler<RoutedEventArgs>? oncbIsMqttEnabledChanged;


        private async void btnMqttSettingsSave_Click(object sender, RoutedEventArgs e)
        {
            var handler = onbtnMqttSettingsSaveClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }

        private async void cbIsMqttEnabledChanged(object sender, RoutedEventArgs e)
        {
            var handler = oncbIsMqttEnabledChanged;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
    }
}

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
    public partial class CustomApi : UserControl
    {
        public CustomApi()
        {
            InitializeComponent();
        }
        public event EventHandler<RoutedEventArgs>? onbtnApiSettingsSaveClick;
        public event EventHandler<RoutedEventArgs>? oncbIsCustomApiEnabledChanged;
        public event EventHandler<SelectionChangedEventArgs>? oncustomApiMethodSelectionChanged;


        private async void btnApiSettingsSave_Click(object sender, RoutedEventArgs e)
        {
            var handler = onbtnApiSettingsSaveClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }

        private async void cbIsCustomApiEnabledChanged(object sender, RoutedEventArgs e)
        {
            var handler = oncbIsCustomApiEnabledChanged;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void customApiMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var handler = oncustomApiMethodSelectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
    }
}

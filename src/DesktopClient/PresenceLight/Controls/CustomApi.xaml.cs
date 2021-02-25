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

using Microsoft.Extensions.Options;

using PresenceLight.Core;
using PresenceLight.Services;

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
        {//NOte: reolved
            //var handler = onbtnApiSettingsSaveClick;
            //if (handler != null)
            //{
            //    handler(this, e);
            //}
            //await Task.CompletedTask;
        }
        private void cbIsCustomApiEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (SettingsHandlerBase.Config.LightSettings.CustomApi.IsEnabled)
            {
                Visibility = Visibility.Visible;
            }
            else
            {
                 Visibility = Visibility.Collapsed;
            }

            
            e.Handled = true;
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

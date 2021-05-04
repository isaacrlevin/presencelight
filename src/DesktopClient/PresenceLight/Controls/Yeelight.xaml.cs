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
    /// Interaction logic for Yeelight.xaml
    /// </summary>
    public partial class Yeelight : UserControl
    {
        public Yeelight()
        {
            InitializeComponent();
        }
        public event EventHandler<RoutedEventArgs>? oncbYeelightIsDisabledChange;
        public event EventHandler<RoutedEventArgs>? oncblsYeelightEnabledChanged;
        public event EventHandler<RoutedEventArgs>? onFindYeelightsClick;
        public event EventHandler<RoutedEventArgs>? onSaveYeelightsClick;
        public event EventHandler<SelectionChangedEventArgs>? onddlYeelightLightsSelectionChanged;
        public event EventHandler<RoutedEventArgs>? oncbUseYeelightActivityStatus;


        private async void cbUseYeelightActivityStatus(object sender, RoutedEventArgs e)
        {
            var handler = oncbUseYeelightActivityStatus;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void cbYeelightIsDisabledChange(object sender, RoutedEventArgs e)
        {
            var handler = oncbYeelightIsDisabledChange;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void cbIsYeelightEnabledChanged(object sender, RoutedEventArgs e)
        {
            var handler = oncblsYeelightEnabledChanged;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void FindYeelights_Click(object sender, RoutedEventArgs e)
        {
            var handler = onFindYeelightsClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void SaveYeelight_Click(object sender, RoutedEventArgs e)
        {
            var handler = onSaveYeelightsClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }

        private void ddlYeelightLights_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var handler = onddlYeelightLightsSelectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
            
        }
    }
}

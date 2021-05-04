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
    /// Interaction logic for PhilipsHue.xaml
    /// </summary>
    public partial class PhilipsHue : UserControl
    {
        public PhilipsHue()
        {
            InitializeComponent();
        }


        public event EventHandler<RoutedEventArgs>? oncbHueIsDisabledChange;
        public event EventHandler<RoutedEventArgs>? oncbIsPhilipsEnabledChanged;
        public event EventHandler<RoutedEventArgs>? oncbUseHueActivityStatus;
        public event EventHandler<RoutedEventArgs>? oncbUseRemoteApiChanged;
        public event EventHandler<RoutedEventArgs>? onFindBridgeClick;
        public event EventHandler<RoutedEventArgs>? onSaveHueClick;
        public event EventHandler<RoutedEventArgs>? onHueApiKeyGet;
        public event EventHandler<RoutedEventArgs>? onRegisterBridgeClick;
        public event EventHandler<RoutedEventArgs>? onCheckHueClick;
        public event EventHandler<TextChangedEventArgs>? onHueIpAddressTextChanged;
        public event EventHandler<SelectionChangedEventArgs>? onddlHueLightsSelectionChanged;

        private void cbHueIsDisabledChange(object sender, RoutedEventArgs e)
        {
            var handler = oncbHueIsDisabledChange;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        private void cbIsPhilipsEnabledChanged(object sender, RoutedEventArgs e)
        {
            var handler = oncbIsPhilipsEnabledChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        private void cbUseHueActivityStatus(object sender, RoutedEventArgs e)
        {
            var handler = oncbUseHueActivityStatus;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void cbUseRemoteApiChanged(object sender, RoutedEventArgs e)
        {
            var handler = oncbUseRemoteApiChanged;
            if (handler != null)
            {
                handler(this, e);
            } 
        }
        private async void FindBridge_Click(object sender, RoutedEventArgs e)
        {
            var handler = onFindBridgeClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void SaveHue_Click(object sender, RoutedEventArgs e)
        {
            var handler = onSaveHueClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void RegisterBridge_Click(object sender, RoutedEventArgs e)
        {
            var handler = onRegisterBridgeClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }

        private async void CheckHue_Click(object sender, RoutedEventArgs e)
        {
            var handler = onCheckHueClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }

        private async void HueIpAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            var handler = onHueIpAddressTextChanged;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        

        private void ddlHueLights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var handler = onddlHueLightsSelectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
           
        }

        private async void HueApiKey_Get(object sender, RoutedEventArgs e)
        {
            var handler = onHueApiKeyGet;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
    }
}

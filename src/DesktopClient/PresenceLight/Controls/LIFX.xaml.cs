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
    /// Interaction logic for LIFX.xaml
    /// </summary>
    public partial class LIFX : UserControl
    {
        public LIFX()
        {
            InitializeComponent();
        }

        public event EventHandler<RoutedEventArgs>? oncbIsLIFXEnabledChanged;
        public event EventHandler<RoutedEventArgs>? oncbLIFXIsDisabledChange;
        public event EventHandler<RoutedEventArgs>? oncbUseLIFXActivityStatus;
        public event EventHandler<RoutedEventArgs>? onCheckLIFXClick;
        public event EventHandler<RequestNavigateEventArgs>? onLIFXTokenGet;
        public event EventHandler<RoutedEventArgs>? onSaveLIFXClick;
        public event EventHandler<SelectionChangedEventArgs>? onddlLIFXLightsSelectionChanged;


        private async void cbIsLIFXEnabledChanged(object sender, RoutedEventArgs e)
        {
            var handler = oncbIsLIFXEnabledChanged;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }

        private async void cbLIFXIsDisabledChange(object sender, RoutedEventArgs e)
        {
            var handler = oncbLIFXIsDisabledChange;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void cbUseLIFXActivityStatus(object sender, RoutedEventArgs e)
        {
            var handler = oncbUseLIFXActivityStatus;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void CheckLIFX_Click(object sender, RoutedEventArgs e)
        {
            var handler = onCheckLIFXClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void LIFXToken_Get(object sender, RequestNavigateEventArgs e)
        {
            var handler = onLIFXTokenGet;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void SaveLIFX_Click(object sender, RoutedEventArgs e)
        {
            var handler = onSaveLIFXClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void ddlLIFXLights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var handler = onddlLIFXLightsSelectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
    }
}

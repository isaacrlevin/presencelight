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
    /// Interaction logic for Wiz.xaml
    /// </summary>
    public partial class Wiz : UserControl
    {
        public Wiz()
        {
            InitializeComponent();
        }
        public event EventHandler<RoutedEventArgs>? oncbWizIsDisabledChange;
        public event EventHandler<RoutedEventArgs>? oncblsWizEnabledChanged;
        public event EventHandler<RoutedEventArgs>? onFindWizsClick;
        public event EventHandler<RoutedEventArgs>? onSaveWizsClick;
        public event EventHandler<SelectionChangedEventArgs>? onddlWizLightsSelectionChanged;
        public event EventHandler<RoutedEventArgs>? oncbUseWizActivityStatus;


        private async void cbUseWizActivityStatus(object sender, RoutedEventArgs e)
        {
            var handler = oncbUseWizActivityStatus;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void cbWizIsDisabledChange(object sender, RoutedEventArgs e)
        {
            var handler = oncbWizIsDisabledChange;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void cbIsWizEnabledChanged(object sender, RoutedEventArgs e)
        {
            var handler = oncblsWizEnabledChanged;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void FindWizs_Click(object sender, RoutedEventArgs e)
        {
            var handler = onFindWizsClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void SaveWiz_Click(object sender, RoutedEventArgs e)
        {
            var handler = onSaveWizsClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }

        private void ddlWizLights_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var handler = onddlWizLightsSelectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
            
        }
    }
}

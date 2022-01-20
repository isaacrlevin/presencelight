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
    /// Interaction logic for LightColors.xaml
    /// </summary>
    public partial class LightColors : UserControl
    {
        public LightColors()
        {
            InitializeComponent();
        }

        public event EventHandler<RoutedEventArgs>? onSetColorClick;
        public event EventHandler<RoutedEventArgs>? onSetTeamsPresenceClick;
        public event EventHandler<RoutedEventArgs>? onSyncThemeClick;


        private async void SetColor_Click(object sender, RoutedEventArgs e)
        {
            var handler = onSetColorClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void SetTeamsPresence_Click(object sender, RoutedEventArgs e)
        {
            var handler = onSetTeamsPresenceClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void SyncTheme_Click(object sender, RoutedEventArgs e)
        {
            var handler = onSyncThemeClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
    }
}

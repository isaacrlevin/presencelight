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
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : UserControl
    {
        public About()
        {
            InitializeComponent();
        }
        public event EventHandler<RequestNavigateEventArgs>? onHyperlinkRequestNavigate;
        public event EventHandler<RoutedEventArgs>? onSettingsLinkClick;
        public event EventHandler<RoutedEventArgs>? onUpdateClick;

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var handler = onHyperlinkRequestNavigate;
            if (handler != null)
            {
                handler(this, e);
            }
        }


        private void SettingsClicked(object sender, RoutedEventArgs e)
        {
            var handler = onSettingsLinkClick;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private async void UpdateClick(object sender, RoutedEventArgs e)
        {
            var handler = onUpdateClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
    }
}

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

using Microsoft.Extensions.Logging;

namespace PresenceLight.Controls
{
    /// <summary>
    /// Interaction logic for LandingPage.xaml
    /// </summary>
    public partial class LandingPage : UserControl
    {
        public LandingPage()
        {
            InitializeComponent();
        }


        public event EventHandler<RoutedEventArgs>? onSignInClicked;
        public event EventHandler<RoutedEventArgs>? onSignOutClicked;
        public event EventHandler<RequestNavigateEventArgs>? onRequestNavigate;
        public event EventHandler<RoutedEventArgs>? onExitClick;
        public event EventHandler<RoutedEventArgs>? onOpenClick;
        public event EventHandler<RoutedEventArgs>? onTurnOffSync;
        public event EventHandler<RoutedEventArgs>? onTurnOnSync;
        public event EventHandler<MouseButtonEventArgs>? onNotifyIconDoubleClick;


        private async void SignIn_Click(object sender, RoutedEventArgs e)
        {
            var handler = onSignInClicked;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void OnExitClick(object sender, RoutedEventArgs e)
        {
            var handler = onExitClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void OnOpenClick(object sender, RoutedEventArgs e)
        {
            var handler = onOpenClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void OnTurnOffSyncClick(object sender, RoutedEventArgs e)
        {
            var handler = onTurnOffSync;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }

        private async void OnNotifyIconDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var handler = onNotifyIconDoubleClick;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void OnTurnOnSyncClick(object sender, RoutedEventArgs e)
        {
            var handler = onTurnOnSync;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            var handler = onSignOutClicked;
            if (handler != null)
            {
                handler(this, e);
            }
            await Task.CompletedTask;
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var handler = onRequestNavigate;
            if (handler != null)
            {
                handler(this, e);
            } 
        }
    }
}

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
    /// Interaction logic for SignIn.xaml
    /// </summary>
    public   partial class SignIn : UserControl
    {
        public SignIn()
        {
            InitializeComponent();
        }

        public event EventHandler<RoutedEventArgs> signinClicked;
        public event EventHandler<RequestNavigateEventArgs> requestNavigate;

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            var handler = signinClicked;
            if (handler != null)
            {
                handler(this, e);
            }
            //TODO Fire an Event
            //await CallGraph().ConfigureAwait(true);
        }
        private async void OnExitClick(object sender, RoutedEventArgs e)
        {

        }
        private async void OnOpenClick(object sender, RoutedEventArgs e)
        {

        }
        private async void OnTurnOffSyncClick(object sender, RoutedEventArgs e)
        {

        }
        private async void OnTurnOnSyncClick(object sender, RoutedEventArgs e)
        {

        }
        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var handler = requestNavigate;
            if (handler != null)
            {
                handler(this, e);
            }
           
        }
    }
}

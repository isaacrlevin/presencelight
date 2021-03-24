using System.Windows.Markup;

using Microsoft.Extensions.DependencyInjection;

using PresenceLight.ViewModels;

namespace PresenceLight.Pages
{
    /// <summary>
    /// Interaction logic for CustomApiPage.xaml
    /// </summary>
    [ContentProperty("Content")]
    public partial class CustomApiPage
    {
        public CustomApiPage()
        {
            InitializeComponent();

            DataContext = App.ServiceProvider.GetRequiredService<CustomApiVm>();
            
            //string response = _mediator.Send(new Core.CustomApiServices.SetColorCommand() { Activity = activity, Availability = color });

            //    var customapi = System.Windows.Application.Current.Windows.OfType<Pages.CustomApiPage>().First();

            //    //TODO: Fix this so it works!
            //    customapi.customApiLastResponse.Content = response;
            //    if (response.Contains("Error:", StringComparison.OrdinalIgnoreCase))
            //    {
            //        customapi.customApiLastResponse.Foreground = new SolidColorBrush(Colors.Red);
            //    }
            //    else
            //    {
            //        customapi.customApiLastResponse.Foreground = new SolidColorBrush(Colors.Green);
            //    }
        }
    }
}

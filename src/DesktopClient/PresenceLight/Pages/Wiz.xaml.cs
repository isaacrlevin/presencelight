using Microsoft.Extensions.DependencyInjection;
using PresenceLight.ViewModels;

namespace PresenceLight.Pages
{
    /// <summary>
    /// Interaction logic for xaml
    /// </summary>
    public partial class Wiz
    {
        public Wiz()
        {
            InitializeComponent();

            DataContext = App.ServiceProvider.GetRequiredService<WizVm>();
        }
    }
}

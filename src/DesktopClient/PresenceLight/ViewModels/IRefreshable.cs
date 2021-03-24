using System.Threading.Tasks;

namespace PresenceLight.ViewModels
{
    public interface IRefreshable
    {
        Task Refresh();
    }
}

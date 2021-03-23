using System.Threading;
using System.Threading.Tasks;

using MediatR;

using PresenceLight.Core.PubSub;

namespace PresenceLight.ViewModels
{
    public class CustomApiInitializeHandler : INotificationHandler<InitializeNotification>
    {
        private readonly CustomApiVm _customApiVm;

        public CustomApiInitializeHandler(CustomApiVm customApiVm)
        {
            _customApiVm = customApiVm;
        }

        public Task Handle(InitializeNotification notification, CancellationToken cancellationToken)
        {
            _customApiVm.Refresh();
            return Task.CompletedTask;
        }
    }
}

using System.Threading;
using System.Threading.Tasks;

using MediatR;

using PresenceLight.Core.CustomApiServices;

namespace PresenceLight.ViewModels
{
    public class CustomApiResponseHandler : INotificationHandler<CustomApiResponseNotification>
    {
        private readonly CustomApiVm _customApiVm;

        public CustomApiResponseHandler(CustomApiVm customApiVm)
        {
            _customApiVm = customApiVm;
        }

        public Task Handle(CustomApiResponseNotification notification, CancellationToken cancellationToken)
        {
            _customApiVm.UpdateLastResponse(notification.Respone);
            return Task.CompletedTask;
        }
    }
}

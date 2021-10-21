using System.Threading;
using System.Threading.Tasks;

using MediatR;

namespace PresenceLight.Core.MqttServices
{
    internal class SetColorHandler : IRequestHandler<SetColorCommand, Unit>
    {
        private readonly MqttNotificationChannel _notificationChannel;
        public SetColorHandler(MqttNotificationChannel notificationChannel)
        {
            _notificationChannel = notificationChannel;
        }

        public async Task<Unit> Handle(SetColorCommand command, CancellationToken cancellationToken)
        {
            await _notificationChannel.NotifyAsync(command.Availability, command.Activity, command.UserName);
            return default;
        }
    }
}

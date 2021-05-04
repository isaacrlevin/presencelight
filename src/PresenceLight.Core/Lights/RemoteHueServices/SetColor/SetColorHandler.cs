using MediatR;

using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.RemoteHueServices
{
    internal class SetColorHandler : IRequestHandler<SetColorCommand, Unit>
    {
        readonly IRemoteHueService _service;
        public SetColorHandler(IRemoteHueService service)
        {
            _service = service;
        }

        public async Task<Unit> Handle(SetColorCommand command, CancellationToken cancellationToken)
        {
            await _service.SetColor(command.Availability, command.LightId, command.BridgeId);
            return default;
        }
    }
}

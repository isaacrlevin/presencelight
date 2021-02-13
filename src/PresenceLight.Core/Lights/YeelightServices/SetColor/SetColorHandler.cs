using MediatR;

using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.YeelightServices
{
    internal class SetColorHandler : IRequestHandler<SetColorCommand, Unit>
    {
        IYeelightService _service;
        public SetColorHandler(IYeelightService service)
        {
            _service = service;
        }

        public async Task<Unit> Handle(SetColorCommand command, CancellationToken cancellationToken)
        {
            await _service.SetColor(command.Availability, command.Activity, command.LightId);
            return default;
        }
    }
}

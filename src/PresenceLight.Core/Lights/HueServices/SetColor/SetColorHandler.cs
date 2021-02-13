using MediatR;

using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.HueServices.HueService
{
    public class SetColorHandler : IRequestHandler<SetColorCommand, Unit>
    {
        IHueService _service;
        public SetColorHandler(IHueService hueService)
        {
            _service = hueService;
        }

        public  async Task<Unit> Handle(SetColorCommand command, CancellationToken cancellationToken)
        {
            await _service.SetColor(command.Availability, command.Activity, command.LightID);
            return default;
        }
    }
}

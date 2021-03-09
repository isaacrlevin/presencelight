using MediatR;

using System.Threading;
using System.Threading.Tasks;

using YeelightAPI;

namespace PresenceLight.Core.YeelightServices
{
    internal class GetLightsHandler : IRequestHandler<GetLightCommand, DeviceGroup>
    {
        IYeelightService _service;
        public GetLightsHandler(IYeelightService service)
        {
            _service = service;
        }

        public async Task<DeviceGroup> Handle(GetLightCommand command, CancellationToken cancellationToken)
        {
            return await _service.FindLights();
        }
    }
}

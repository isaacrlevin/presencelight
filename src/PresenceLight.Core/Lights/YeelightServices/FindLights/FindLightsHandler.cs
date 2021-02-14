using MediatR;

using System.Threading;
using System.Threading.Tasks;

using YeelightAPI;

namespace PresenceLight.Core.YeelightServices
{
    internal class FindLightsHandler : IRequestHandler<FindLightsCommand, DeviceGroup>
    {
        IYeelightService _service;
        public FindLightsHandler(IYeelightService service)
        {
            _service = service;
        }

        public async Task<DeviceGroup> Handle(FindLightsCommand command, CancellationToken cancellationToken)
        {
            return await _service.FindLights();
        }
    }
}

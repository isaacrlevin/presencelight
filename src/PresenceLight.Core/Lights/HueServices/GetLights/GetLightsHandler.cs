using MediatR;
using Q42.HueApi;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.HueServices
{
    public class GetLightsHandler : IRequestHandler<GetLightsCommand, IEnumerable<Light>>
    {
        IHueService _service;
        public GetLightsHandler(IHueService hueService)
        {
            _service = hueService;
        }

        public async Task<IEnumerable<Light>> Handle(GetLightsCommand command, CancellationToken cancellationToken)
        {
            return await _service.GetLights();
        }
    }
}

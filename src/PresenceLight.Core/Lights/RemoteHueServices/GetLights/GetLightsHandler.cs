using MediatR;
using Q42.HueApi;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.RemoteHueServices
{
    internal class GetLightsHandler : IRequestHandler<GetLightsCommand, IEnumerable<Light>>
    {
        readonly IRemoteHueService _service;
        public GetLightsHandler(IRemoteHueService service)
        {
            _service = service;
        }

        public async Task<IEnumerable<Light>> Handle(GetLightsCommand command, CancellationToken cancellationToken)
        {
            return await _service.GetLights();
        }
    }
}

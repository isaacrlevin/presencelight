using LifxCloud.NET;
using LifxCloud.NET.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using PresenceLight.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.LifxServices
{
    internal class GetAllLightsHandler : IRequestHandler<GetAllLightsCommand, List<Light>>
    {
        LIFXService _service;
        public GetAllLightsHandler(LIFXService service)
        {
            _service = service;
        }

        public async Task<List<Light>> Handle(GetAllLightsCommand command, CancellationToken cancellationToken)
        {
            return await _service.GetAllLights(command.ApiKey);
        }
    }
}

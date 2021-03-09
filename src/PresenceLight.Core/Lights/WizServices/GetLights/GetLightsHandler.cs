using MediatR;

using OpenWiz;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.WizServices
{
    public class GetLightsHandler : IRequestHandler<GetLightsCommand, IEnumerable<WizLight>>
    {
        IWizService _service;
        public GetLightsHandler(IWizService service)
        {
            _service = service;
        }

        public async Task<IEnumerable<WizLight>> Handle(GetLightsCommand command, CancellationToken cancellationToken)
        {
            return await _service.GetLights();
        }
    }
}

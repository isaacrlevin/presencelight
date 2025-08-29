using MediatR;

using OpenWiz;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.WizServices
{
    public class GetLightHandler : IRequestHandler<GetLightCommand, WizLight>
    {
        IWizService _service;
        public GetLightHandler(IWizService service)
        {
            _service = service;
        }

        public async Task<WizLight> Handle(GetLightCommand command, CancellationToken cancellationToken)
        {
            return await _service.GetLight();
        }
    }
}

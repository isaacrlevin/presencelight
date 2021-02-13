using LifxCloud.NET;

using MediatR;

using Microsoft.Extensions.Logging;

using PresenceLight.Core;

using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.LifxServices
{
    internal class SetColorHandler : IRequestHandler<SetColorCommand, Unit>
    {
        LIFXService _service;
        public SetColorHandler(LIFXService service)
        {
            _service = service;
        }

        public async Task<Unit> Handle(SetColorCommand command, CancellationToken cancellationToken)
        {
            await _service.SetColor(command.Availability, command.Activity, command.LightId, command.ApiKey);
            return default;
        }
    }
}

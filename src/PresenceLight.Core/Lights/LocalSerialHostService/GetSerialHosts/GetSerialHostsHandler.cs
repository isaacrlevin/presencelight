using MediatR;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.LocalSerialHostServices
{

    internal class GetAvailablePortsHandler : IRequestHandler<GetPortCommand, IEnumerable<string>>
    {
        ILocalSerialHostService _service;

        public GetAvailablePortsHandler(ILocalSerialHostService service)
        {
            _service = service;
        }

        public async Task<IEnumerable<string>> Handle(GetPortCommand command, CancellationToken cancellationToken)
        {
            return await _service.GetPorts();
        }
    }
}
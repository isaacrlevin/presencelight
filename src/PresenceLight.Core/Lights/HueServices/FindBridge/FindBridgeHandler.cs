using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.HueServices
{
    public class FindBridgeHandler : IRequestHandler<FindBridgeCommand, string>
    {
        IHueService _service;
        public FindBridgeHandler(IHueService hueService)
        {
            _service = hueService;
        }

        public async Task<string> Handle(FindBridgeCommand command, CancellationToken cancellationToken)
        {
            return await _service.FindBridge();
        }
    }
}

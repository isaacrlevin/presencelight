using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.HueServices
{
    public class RegisterBridgeHandler : IRequestHandler<RegisterBridgeCommand, string>
    {
        IHueService _service;
        public RegisterBridgeHandler(IHueService hueService)
        {
            _service = hueService;
        }

        public async Task<string> Handle(RegisterBridgeCommand command, CancellationToken cancellationToken)
        {
            return await _service.RegisterBridge();
        }
    }
}

using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.RemoteHueServices
{
    internal class RegisterBridgeHandler : IRequestHandler<RegisterBridgeCommand, (string bridgeId, string apiKey, string bridgeIp)>
    {
        readonly IRemoteHueService _service;
        public RegisterBridgeHandler(IRemoteHueService service)
        {
            _service = service;
        }

        public async Task<(string bridgeId, string apiKey, string bridgeIp)> Handle(RegisterBridgeCommand command, CancellationToken cancellationToken)
        {
            return await _service.RegisterBridge();
        }
    }
}

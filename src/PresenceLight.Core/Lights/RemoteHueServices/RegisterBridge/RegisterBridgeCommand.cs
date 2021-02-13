using MediatR;
using System;

namespace PresenceLight.Core.RemoteHueServices
{
    public class RegisterBridgeCommand : IRequest<(string bridgeId, string apiKey, string bridgeIp)>
    {
    }
}

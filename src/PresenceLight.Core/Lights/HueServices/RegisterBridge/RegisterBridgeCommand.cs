using MediatR;
using System;

namespace PresenceLight.Core.HueServices
{
    public class RegisterBridgeCommand : IRequest<string>
    {
    }
}

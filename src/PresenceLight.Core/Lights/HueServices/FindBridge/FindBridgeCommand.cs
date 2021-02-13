using MediatR;
using System;

namespace PresenceLight.Core.HueServices
{
    public class FindBridgeCommand : IRequest<string>
    {
    }
}

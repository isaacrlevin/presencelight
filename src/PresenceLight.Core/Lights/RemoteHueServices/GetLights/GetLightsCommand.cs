using MediatR;
using HueApi.Models;
using System.Collections.Generic;

namespace PresenceLight.Core.RemoteHueServices
{
    public class GetLightsCommand : IRequest<IEnumerable<Light>>
    {
    }
}

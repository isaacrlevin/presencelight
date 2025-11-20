using MediatR;
using HueApi.Models;
using System.Collections.Generic;

namespace PresenceLight.Core.HueServices
{
    public class GetLightsCommand : IRequest<IEnumerable<Light>>
    {
    }
}

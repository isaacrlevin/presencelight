using LifxCloud.NET.Models;

using MediatR;

using System.Collections.Generic;

namespace PresenceLight.Core.LifxServices
{
    public class GetAllLightsCommand : IRequest<List<Light>>
    {
        public string ApiKey { get; set; }
    }
}

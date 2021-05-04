using LifxCloud.NET.Models;

using MediatR;

using System.Collections.Generic;

namespace PresenceLight.Core.LifxServices
{
    public class GetAllGroupsCommand : IRequest<List<Group>>
    {
        public string ApiKey { get; set; }
    }
}

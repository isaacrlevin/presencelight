using HueApi.Models;

using MediatR;
using System.Collections.Generic;

namespace PresenceLight.Core.HueServices
{
    public class GetGroupsCommand : IRequest<IEnumerable<GroupedLight>>
    {
    }
}

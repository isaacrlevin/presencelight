using MediatR;
using Q42.HueApi;
using Q42.HueApi.Models.Groups;

using System.Collections.Generic;

namespace PresenceLight.Core.RemoteHueServices
{
    public class GetGroupsCommand : IRequest<IEnumerable<Group>>
    {
    }
}

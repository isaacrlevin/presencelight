using MediatR;
using Q42.HueApi;
using Q42.HueApi.Models.Groups;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.RemoteHueServices
{
    internal class GetGroupsHandler : IRequestHandler<GetGroupsCommand, IEnumerable<Group>>
    {
        readonly IRemoteHueService _service;
        public GetGroupsHandler(IRemoteHueService service)
        {
            _service = service;
        }

        public async Task<IEnumerable<Group>> Handle(GetGroupsCommand command, CancellationToken cancellationToken)
        {
            return await _service.GetGroups();
        }
    }
}

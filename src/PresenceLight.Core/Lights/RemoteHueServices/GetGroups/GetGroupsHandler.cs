using MediatR;
using HueApi.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.RemoteHueServices
{
    internal class GetGroupsHandler : IRequestHandler<GetGroupsCommand, IEnumerable<GroupedLight>>
    {
        readonly IRemoteHueService _service;
        public GetGroupsHandler(IRemoteHueService service)
        {
            _service = service;
        }

        public async Task<IEnumerable<GroupedLight>> Handle(GetGroupsCommand command, CancellationToken cancellationToken)
        {
            return await _service.GetGroups();
        }
    }
}

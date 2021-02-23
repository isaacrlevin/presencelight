using MediatR;
using Q42.HueApi;
using Q42.HueApi.Models.Groups;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.HueServices
{
    public class GetGroupsHandler : IRequestHandler<GetGroupsCommand, IEnumerable<Group>>
    {
        IHueService _service;
        public GetGroupsHandler(IHueService hueService)
        {
            _service = hueService;
        }

        public async Task<IEnumerable<Group>> Handle(GetGroupsCommand command, CancellationToken cancellationToken)
        {
            return await _service.GetGroups();
        }
    }
}

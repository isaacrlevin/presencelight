using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using HueApi.Models;

using MediatR;


namespace PresenceLight.Core.HueServices
{
    public class GetGroupsHandler : IRequestHandler<GetGroupsCommand, IEnumerable<GroupedLight>>
    {
        IHueService _service;
        public GetGroupsHandler(IHueService hueService)
        {
            _service = hueService;
        }

        public async Task<IEnumerable<GroupedLight>> Handle(GetGroupsCommand command, CancellationToken cancellationToken)
        {
            return await _service.GetGroups();
        }
    }
}

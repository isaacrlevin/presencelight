using LifxCloud.NET;
using LifxCloud.NET.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using PresenceLight.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.LifxServices
{
    internal class GetAllGroupsHandler : IRequestHandler<GetAllGroupsCommand, List<Group>>
    {
        LIFXService _service;
        public GetAllGroupsHandler(LIFXService service)
        {
            _service = service;
        }

        public async Task<List<Group>> Handle(GetAllGroupsCommand command, CancellationToken cancellationToken)
        {
            return await _service.GetAllGroups(command.ApiKey);
        }
    }
}

using MediatR;
using Microsoft.Graph;

using Polly;
using Polly.Retry;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.GraphServices
{
    internal class GetProfileAndPresenceHandler : IRequestHandler<GetProfileAndPresenceCommand, (User User, Presence Presence)>
    {
        GraphWrapper _graph;

        public GetProfileAndPresenceHandler(GraphWrapper graph)
        {
            _graph = graph;
        }

        public async Task<(User User, Presence Presence)> Handle(GetProfileAndPresenceCommand command, CancellationToken cancellationToken)
        {
           return await  _graph.GetProfileAndPresence(cancellationToken);
        }
    }
}

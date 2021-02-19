using MediatR;
using Microsoft.Graph;
using Polly.Retry;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.GraphServices
{
    internal class GetPresenceHandler : IRequestHandler<GetPresenceCommand, Presence>
    {
        GraphWrapper _graph;

        public GetPresenceHandler(GraphWrapper graph)
        {
            _graph = graph;
        }

        public async Task<Presence> Handle(GetPresenceCommand command, CancellationToken cancellationToken)
        {
            return await _graph.GetPresence(cancellationToken);
        }
    }
}

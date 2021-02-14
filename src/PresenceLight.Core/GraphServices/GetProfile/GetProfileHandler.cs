using MediatR;
using Microsoft.Graph;
using Polly.Retry;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.GraphServices
{
    internal class GetProfileHandler : IRequestHandler<GetProfileCommand, User>
    {

        GraphWrapper _graph;

        public GetProfileHandler(GraphWrapper graph)
        {
            _graph = graph;
        }


        public async Task<User> Handle(GetProfileCommand command, CancellationToken cancellationToken)
        {
            return await _graph.GetProfile(cancellationToken);
        }
    }
}

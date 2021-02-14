using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.GraphServices
{
    internal class GetIsInitializedHandler : IRequestHandler<GetIsInitializedCommand, bool>
    {
        GraphWrapper _graph;

        public GetIsInitializedHandler(GraphWrapper graph)
        {
            _graph = graph;
        }


        public async Task<bool> Handle(GetIsInitializedCommand command, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_graph.IsInitialized);
        }
    }
}

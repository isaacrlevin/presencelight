using MediatR;

using Microsoft.Graph;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.GraphServices
{
    internal class InitializeHandler : IRequestHandler<InitializeCommand>
    {
        GraphWrapper _graph;

        public InitializeHandler(GraphWrapper graph)
        {
            _graph = graph;
        }

        public async Task<Unit> Handle(InitializeCommand command, CancellationToken cancellationToken)
        {
            _graph.Initialize(command.Client);
            await Task.CompletedTask;
            return default;
        }
    }
}

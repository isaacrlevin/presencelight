using MediatR;

using Microsoft.Graph;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.GraphServices
{
    internal class InitializeHandler : AsyncRequestHandler<InitializeCommand>
    {
        GraphWrapper _graph;

        public InitializeHandler(GraphWrapper graph)
        {
            _graph = graph;
        }

        protected async override Task Handle(InitializeCommand command, CancellationToken cancellationToken)
        {
            _graph.Initialize(command.Client);
            await Task.CompletedTask;
        }
    }
}

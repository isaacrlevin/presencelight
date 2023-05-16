using MediatR;

using Microsoft.Graph;

using System;
using System.Threading;
using System.Threading.Tasks;

using YeelightAPI.Models;

namespace PresenceLight.Core.GraphServices
{
    internal class InitializeHandler : IRequestHandler<InitializeCommand>
    {
        GraphWrapper _graph;

        public InitializeHandler(GraphWrapper graph)
        {
            _graph = graph;
        }

        async Task IRequestHandler<InitializeCommand>.Handle(InitializeCommand command, CancellationToken cancellationToken)
        {
            _graph.Initialize(command.Client);
            await Task.CompletedTask;
        }
    }
}

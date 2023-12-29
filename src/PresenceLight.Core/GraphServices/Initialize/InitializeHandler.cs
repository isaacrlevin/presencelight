using System.Threading;
using System.Threading.Tasks;

using MediatR;

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
            await _graph.Initialize();
            await Task.CompletedTask;
        }
    }
}

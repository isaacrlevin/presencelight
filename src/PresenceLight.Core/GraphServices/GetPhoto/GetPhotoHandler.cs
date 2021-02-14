using MediatR;
using Microsoft.Graph;
using Polly.Retry;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.GraphServices
{
    internal class GetPhotoHandler : IRequestHandler<GetPhotoCommand, Stream>
    {
        GraphWrapper _graph;

        public GetPhotoHandler(GraphWrapper graph)
        {
            _graph = graph;
        }

        public async Task<Stream> Handle(GetPhotoCommand command, CancellationToken cancellationToken)
        {
            return await _graph.GetPhoto(cancellationToken);
        }
    }
}

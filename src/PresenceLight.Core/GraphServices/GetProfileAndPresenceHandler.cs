using MediatR;
using Microsoft.Graph.Models;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.GraphServices
{
    internal class GetProfileAndPresenceHandler : IRequestHandler<GetProfileAndPresenceCommand, (User User, Presence Presence, string Email)>
    {
        GraphWrapper _graph;

        public GetProfileAndPresenceHandler(GraphWrapper graph)
        {
            _graph = graph;
        }

        public async Task<(User User, Presence Presence, string Email)> Handle(GetProfileAndPresenceCommand command, CancellationToken cancellationToken)
        {
            var (user, presence) = await _graph.GetProfileAndPresence(cancellationToken);
            var email = user.Mail;
            return (user, presence, email);
        }
    }
}

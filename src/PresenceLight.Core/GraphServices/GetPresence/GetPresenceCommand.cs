using MediatR;
using Microsoft.Graph;

namespace PresenceLight.Core.GraphServices
{
    public class GetPresenceCommand : IRequest<Presence>
    {
    }
}

using MediatR;

using Microsoft.Graph.Models;

namespace PresenceLight.Core.GraphServices
{
    public class GetProfileCommand : IRequest<User>
    {
    }
}

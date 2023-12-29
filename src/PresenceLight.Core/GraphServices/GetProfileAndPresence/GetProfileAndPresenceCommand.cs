using MediatR;

using Microsoft.Graph.Models;

using System;

namespace PresenceLight.Core.GraphServices
{
    public class GetProfileAndPresenceCommand : IRequest<(User User, Presence Presence)>
    {
    }
}

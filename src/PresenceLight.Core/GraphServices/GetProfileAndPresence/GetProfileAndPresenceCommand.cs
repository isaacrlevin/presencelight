using MediatR;
using Microsoft.Graph;
using System;

namespace PresenceLight.Core.GraphServices
{
    public class GetProfileAndPresenceCommand : IRequest<(User User, Presence Presence)>
    {
    }
}

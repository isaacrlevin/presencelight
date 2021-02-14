using MediatR;
using System;

namespace PresenceLight.Core.GraphServices
{
    public class GetIsInitializedCommand : IRequest<bool>
    {
    }
}

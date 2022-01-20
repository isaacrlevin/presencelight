using MediatR;
using System;

namespace PresenceLight.Core.Initialize
{
    public class InitializeCommand : IRequest
    {
        public AppState AppState { get;   set; }
    }
}

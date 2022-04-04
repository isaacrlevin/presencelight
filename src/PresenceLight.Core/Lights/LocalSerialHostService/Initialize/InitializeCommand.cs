using MediatR;
using System;

namespace PresenceLight.Core.LocalSerialHostServices
{
    public class InitializeCommand : IRequest
    {
        public AppState AppState { get;   set; }
    }
}

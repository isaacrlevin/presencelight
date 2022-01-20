using MediatR;
using System;

namespace PresenceLight.Core.RemoteHueServices
{
    public class InitializeCommand : IRequest
    {
        public BaseConfig Options { get;   set; }
    }
}

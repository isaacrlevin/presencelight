using MediatR;
using System;

namespace PresenceLight.Core.Initialize
{
    public class InitializeCommand : IRequest
    {
        public BaseConfig Request { get;   set; }
    }
}

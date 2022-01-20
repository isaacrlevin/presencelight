using MediatR;

using System;

namespace PresenceLight.Core.HueServices
{
    public class InitializeCommand : IRequest
    {
        public BaseConfig Request { get; set; }
    }
}

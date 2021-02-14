using MediatR;
using System;

namespace PresenceLight.Core.LifxServices
{
    public class InitializeCommand : IRequest
    {
        public BaseConfig Options { get; set; }
    }
}

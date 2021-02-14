using MediatR;

using Microsoft.Graph;

using System;

namespace PresenceLight.Core.GraphServices
{
    public class InitializeCommand : IRequest
    {
        public GraphServiceClient Client { get;   set; }
    }
}

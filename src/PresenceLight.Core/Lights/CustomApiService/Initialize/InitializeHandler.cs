using MediatR;

using PresenceLight.Core;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.Initialize
{
    internal class InitializeHandler : IRequestHandler<InitializeCommand>
    {
        readonly ICustomApiService _service;
        public InitializeHandler(ICustomApiService service)
        {
            _service = service;
        }

        public Task<Unit> Handle(InitializeCommand command, CancellationToken cancellationToken)
        {
            _service.Initialize(command.Request);
            return Unit.Task;
        }
    }
}

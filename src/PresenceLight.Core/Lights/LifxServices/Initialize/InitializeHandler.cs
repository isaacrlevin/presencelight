using MediatR;

using PresenceLight.Core;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.LifxServices
{
    internal class InitializeHandler : IRequestHandler<InitializeCommand>
    {
        LIFXService _service;
        public InitializeHandler(LIFXService service)
        {
            _service = service;
        }

        public async Task<Unit> Handle(InitializeCommand command, CancellationToken cancellationToken)
        {
            _service.Initialize(command.Options);
            await Task.CompletedTask;
            return default;
        }
    }
}

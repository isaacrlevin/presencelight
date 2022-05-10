using MediatR;

using PresenceLight.Core;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.LocalSerialHostServices
{
    internal class InitializeHandler : IRequestHandler<InitializeCommand>
    {
        readonly ILocalSerialHostService _service;
        public InitializeHandler(ILocalSerialHostService service)
        {
            _service = service;
            
        }

        public Task<Unit> Handle(InitializeCommand command, CancellationToken cancellationToken)
        {
            _service.Initialize(command.AppState);
            return Unit.Task;
        }
    }
}

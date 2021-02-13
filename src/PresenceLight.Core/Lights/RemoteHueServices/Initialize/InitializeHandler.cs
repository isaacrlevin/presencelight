using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.RemoteHueServices
{
    internal class InitializeHandler : IRequestHandler<InitializeCommand>
    {
        readonly IRemoteHueService _service;
        public InitializeHandler(IRemoteHueService service)
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

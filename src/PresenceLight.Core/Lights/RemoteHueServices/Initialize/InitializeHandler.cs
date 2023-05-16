using MediatR;

using System;
using System.Threading;
using System.Threading.Tasks;

using YeelightAPI.Models;

namespace PresenceLight.Core.RemoteHueServices
{
    internal class InitializeHandler : IRequestHandler<InitializeCommand>
    {
        readonly IRemoteHueService _service;
        public InitializeHandler(IRemoteHueService service)
        {
            _service = service;
        }

        async Task IRequestHandler<InitializeCommand>.Handle(InitializeCommand command, CancellationToken cancellationToken)
        {
            _service.Initialize(command.AppState);
            await Task.CompletedTask;
        }
    }
}

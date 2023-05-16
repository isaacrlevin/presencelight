using MediatR;

using PresenceLight.Core;

using System;
using System.Threading;
using System.Threading.Tasks;

using YeelightAPI.Models;

namespace PresenceLight.Core.LifxServices
{
    internal class InitializeHandler : IRequestHandler<InitializeCommand>
    {
        LIFXService _service;
        public InitializeHandler(LIFXService service)
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

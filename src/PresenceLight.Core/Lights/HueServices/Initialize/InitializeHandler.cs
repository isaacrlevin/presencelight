using MediatR;

using PresenceLight.Core;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.HueServices
{
    internal class InitializeHandler : IRequestHandler<InitializeCommand>
    {
        IHueService _service;
        public InitializeHandler(IHueService hueService)
        {
            _service = hueService;
        }


        public async Task<Unit> Handle(InitializeCommand command, CancellationToken cancellationToken)
        {
            _service.Initialize(command.Request);
            await Task.CompletedTask;
            return default;
        }
    }
}

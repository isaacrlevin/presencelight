using MediatR;

using PresenceLight.Core;

using System;
using System.Threading;
using System.Threading.Tasks;

using YeelightAPI.Models;

namespace PresenceLight.Core.Initialize
{
    internal class InitializeHandler : IRequestHandler<InitializeCommand>
    {
        readonly ICustomApiService _service;
        public InitializeHandler(ICustomApiService service)
        {
            _service = service;
        }

        Task IRequestHandler<InitializeCommand>.Handle(InitializeCommand command, CancellationToken cancellationToken)
        {
            _service.Initialize(command.AppState);
            return Unit.Task;
        }
    }
}

using MediatR;
using PresenceLight.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.CustomApiServices
{
    internal class SetColorHandler : IRequestHandler<SetColorCommand, string>
    {
        readonly ICustomApiService _service;
        public SetColorHandler(ICustomApiService service)
        {
            _service = service;
        }

        public async Task<string> Handle(SetColorCommand command, CancellationToken cancellationToken)
        {
            return await _service.SetColor(command.Availability, command.Activity, cancellationToken);
        }
    }
}

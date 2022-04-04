using MediatR;
using PresenceLight.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.LocalSerialHostServices
{
    internal class SetColorHandler : IRequestHandler<SetColorCommand, string>
    {
        readonly ILocalSerialHostService _service;
        public SetColorHandler(ILocalSerialHostService service)
        {
            _service = service;
        }

        public async Task<string> Handle(SetColorCommand command, CancellationToken cancellationToken)
        {
            return await _service.SetColor(command.Availability, command.Activity, cancellationToken);
        }
    }
}

using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Services 
{
    internal class SetColorHandler : IRequestHandler<SetColorCommand, Unit>
    {
        ColorService service;
        public SetColorHandler( ColorService colorService)
        {
            service = colorService;
        }

        public async Task<Unit> Handle(SetColorCommand command, CancellationToken cancellationToken)
        {
           await  service.SetColor(command.Color, command.Activity);
            return Unit.Value;
        }
    }
}

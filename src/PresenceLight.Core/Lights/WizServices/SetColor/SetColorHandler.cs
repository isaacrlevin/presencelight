using MediatR;

using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.WizServices.WizService
{
    public class SetColorHandler : IRequestHandler<SetColorCommand, Unit>
    {
        IWizService _service;
        public SetColorHandler(IWizService hueService)
        {
            _service = hueService;
        }

        public  async Task<Unit> Handle(SetColorCommand command, CancellationToken cancellationToken)
        {
            await _service.SetColor(command.Availability, command.Activity, command.LightID);
            return default;
        }
    }
}

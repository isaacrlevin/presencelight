using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.WorkingHoursServices
{
    internal class IsInWorkingHoursHandler : IRequestHandler<IsInWorkingHoursCommand, bool>
    {
        IWorkingHoursService _service;
        public IsInWorkingHoursHandler(IWorkingHoursService service)
        {
            _service = service;
        }

        public async Task<bool> Handle(IsInWorkingHoursCommand command, CancellationToken cancellationToken)
        {
            
            return await Task.FromResult(_service.IsInWorkingHours());

        }
    }
}

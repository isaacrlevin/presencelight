using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.WorkingHoursServices
{
    internal class UseWorkingHoursHandler : IRequestHandler<UseWorkingHoursCommand, bool>
    {
        IWorkingHoursService _service;
        public UseWorkingHoursHandler(IWorkingHoursService service)
        {
            _service = service;
        }

        public async Task<bool> Handle(UseWorkingHoursCommand command, CancellationToken cancellationToken)
        {
            
            return await Task.FromResult(_service.UseWorkingHours());

        }
    }
}

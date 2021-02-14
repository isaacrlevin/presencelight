using MediatR;
using System;

namespace PresenceLight.Core.WorkingHoursServices
{
    public class IsInWorkingHoursCommand : IRequest<bool>
    {
    }
}

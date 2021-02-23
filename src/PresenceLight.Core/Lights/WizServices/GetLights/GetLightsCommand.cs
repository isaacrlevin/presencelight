using System.Collections.Generic;

using MediatR;

using OpenWiz;

namespace PresenceLight.Core.WizServices
{
    public class GetLightsCommand : IRequest<IEnumerable<WizLight>>
    {
    }
}

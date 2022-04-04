using MediatR;
using System.Collections.Generic;

namespace PresenceLight.Core.LocalSerialHostServices
{
    public class GetPortCommand : IRequest<IEnumerable<string>>
    {
    }
}

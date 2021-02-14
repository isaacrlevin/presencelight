using MediatR;
using YeelightAPI;

namespace PresenceLight.Core.YeelightServices
{
    public class FindLightsCommand : IRequest<DeviceGroup>
    {
    }
}

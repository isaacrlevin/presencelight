using MediatR;
using YeelightAPI;

namespace PresenceLight.Core.YeelightServices
{
    public class GetLightCommand : IRequest<DeviceGroup>
    {
    }
}

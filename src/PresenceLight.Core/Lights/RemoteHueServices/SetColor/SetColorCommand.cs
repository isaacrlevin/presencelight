using MediatR;
using System.Threading.Tasks;

namespace PresenceLight.Core.RemoteHueServices
{
    public class SetColorCommand : IRequest<Unit>
    {
        public string Availability { get; set; }
        public string LightId { get; set; }
        public string BridgeId { get; set; }
    }
}

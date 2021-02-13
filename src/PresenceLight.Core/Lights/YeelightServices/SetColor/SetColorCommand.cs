using MediatR;

using System.Threading.Tasks;

namespace PresenceLight.Core.YeelightServices
{
    public class SetColorCommand : IRequest<Unit>
    {
        public string Availability { get; set; }
        public string Activity { get; set; }
        public string LightId { get; set; }
    }
}

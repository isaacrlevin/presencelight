using MediatR;

using System.Threading.Tasks;

namespace PresenceLight.Core.LifxServices
{
    public class SetColorCommand : IRequest<Unit>
    {
        public string Availability { get; set; }
        public string Activity { get; set; }
        public string? LightId { get; set; }
        public string ApiKey { get; set; }
    }
}

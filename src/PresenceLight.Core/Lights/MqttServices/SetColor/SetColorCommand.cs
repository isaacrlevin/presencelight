using MediatR;

namespace PresenceLight.Core.MqttServices
{
    public class SetColorCommand : IRequest<Unit>
    {
        public string Availability { get; set; } = "unknown";
        public string Activity { get; set; } = "unknown";
        public string? UserName { get; set; }
    }
}

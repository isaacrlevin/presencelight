using MediatR;

using Microsoft.Graph;

namespace PresenceLight.Core
{
    public class SetColorNotification : INotification
    {
        public string Availability { get; }
        public string? Activity { get; }

        public SetColorNotification(Presence presence)
            : this(presence.Availability, presence.Activity)
        {   
        }

        public SetColorNotification(string availability, string activity = "")
        {
            Availability = availability;
            Activity = activity;
        }
    }
}

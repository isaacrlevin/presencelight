using System;

using Newtonsoft.Json;

namespace PresenceLight.Core.MqttServices
{
    internal class PresenceMessagePayload : IEquatable<PresenceMessagePayload>
    {
        public string Availability { get; }
        public string Activity { get; }
        public string? UserName { get; }

        public PresenceMessagePayload(string availability, string activity, string? userName)
        {
            Availability = availability;
            Activity = activity;
            UserName = userName;
        }


        public string AsJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public bool Equals(PresenceMessagePayload? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Availability == other.Availability && Activity == other.Activity && UserName == other.UserName;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PresenceMessagePayload)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Availability, Activity, UserName);
        }
    }
}

namespace PresenceLight.Core
{
    public class Subscription
    {
        public string? Availability { get; set; }
        public string? Activity { get; set; }

        public virtual bool IsValid() => true;
    }
}

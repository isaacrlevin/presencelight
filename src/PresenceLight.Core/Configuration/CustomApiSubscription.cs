namespace PresenceLight.Core
{
    public class CustomApiSubscription : Subscription
    {
        public string Method { get; set; }
        public string Uri { get; set; }

        public override bool IsValid() => !string.IsNullOrWhiteSpace(Method) && !string.IsNullOrWhiteSpace(Uri);
    }
}

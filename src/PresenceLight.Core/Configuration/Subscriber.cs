using System.Collections.Generic;

namespace PresenceLight.Core
{
    public class Subscriber<TSubscription>
        where TSubscription: Subscription
    {
        public bool IsEnabled { get; set; }

        public List<CustomApiSubscription> Subscriptions { get; set; } = new List<CustomApiSubscription>();
    }
}

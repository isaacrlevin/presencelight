using System.Collections.Generic;

namespace PresenceLight.Core
{
    public class CustomApi : BaseLight
    {
        public double CustomApiTimeout { get; set; }

        public List<CustomApiSetting> Subscriptions { get; set; } = new List<CustomApiSetting>();
    }
}

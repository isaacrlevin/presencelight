using System;
using System.Collections.Generic;
using System.Text;

namespace PresenceLight.Core
{
    public class ConfigWrapper
    {
        public string ApplicationId { get; set; }
        public string ApplicationSecret { get; set; }
        public string TenantId { get; set; }
        public string RedirectUri { get; set; }
        public string HueApiKey { get; set; }
        public string HueIpAddress { get; set; }
    }
}

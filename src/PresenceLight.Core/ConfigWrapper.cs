using System.ComponentModel.DataAnnotations;

namespace PresenceLight.Core
{
    public class ConfigWrapper
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Domain { get; set; }

        public string Instance { get; set; }
        public string TenantId { get; set; }

        public string RedirectUri { get; set; }
        public string CallbackPath { get; set; }
        public string HueApiKey { get; set; }
        public string SelectedLightId { get; set; }
        public string HueIpAddress { get; set; }
        public string IconType { get; set; }
    }
}

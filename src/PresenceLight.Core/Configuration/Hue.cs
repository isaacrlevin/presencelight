using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace PresenceLight.Core
{
    public class Hue : BaseLight
    {
        public string? RemoteHueClientId { get; set; }

        public string? RemoteHueClientAppName { get; set; }

        public string? RemoteHueClientSecret { get; set; }
        public string? HueApiKey { get; set; }

        [Required]
        [RegularExpression(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b",
        ErrorMessage = "Not a valid IP Address")]
        public string? HueIpAddress { get; set; }

        public bool UseRemoteApi { get; set; }

        public string RemoteBridgeId { get; set; }
    }
}

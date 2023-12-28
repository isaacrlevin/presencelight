using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace PresenceLight.Core
{
    /// <summary>
    /// Represents the configuration for Hue lights.
    /// </summary>
    public class Hue : BaseLight
    {
        /// <summary>
        /// Gets or sets the client ID for remote Hue access.
        /// </summary>
        public string? RemoteHueClientId { get; set; }

        /// <summary>
        /// Gets or sets the client application name for remote Hue access.
        /// </summary>
        public string? RemoteHueClientAppName { get; set; }

        /// <summary>
        /// Gets or sets the client secret for remote Hue access.
        /// </summary>
        public string? RemoteHueClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the API key for local Hue access.
        /// </summary>
        public string? HueApiKey { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the Hue bridge.
        /// </summary>
        [Required]
        [RegularExpression(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b",
        ErrorMessage = "Not a valid IP Address")]
        public string? HueIpAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use remote API for Hue access.
        /// </summary>
        public bool UseRemoteApi { get; set; }

        /// <summary>
        /// Gets or sets the bridge ID for remote Hue access.
        /// </summary>
        public string RemoteBridgeId { get; set; }
    }
}

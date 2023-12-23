namespace PresenceLight.Core
{
    /// <summary>
    /// Represents the configuration for LIFX lights.
    /// </summary>
    public class LIFX : BaseLight
    {
        /// <summary>
        /// Gets or sets the LIFX API key.
        /// </summary>
        public string? LIFXApiKey { get; set; }

        /// <summary>
        /// Gets or sets the LIFX client ID.
        /// </summary>
        public string? LIFXClientId { get; set; }

        /// <summary>
        /// Gets or sets the LIFX client secret.
        /// </summary>
        public string? LIFXClientSecret { get; set; }
    }
}

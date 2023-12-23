namespace PresenceLight.Core
{
    /// <summary>
    /// Represents the settings for a custom API.
    /// </summary>
    public class CustomApiSetting
    {
        /// <summary>
        /// Gets or sets the HTTP method used for the API.
        /// </summary>
        public string? Method { get; set; }

        /// <summary>
        /// Gets or sets the URI of the API.
        /// </summary>
        public string? Uri { get; set; }
    }
}

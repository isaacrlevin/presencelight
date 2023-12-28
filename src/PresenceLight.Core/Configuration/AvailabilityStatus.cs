namespace PresenceLight.Core
{
    /// <summary>
    /// Represents the availability status configuration.
    /// </summary>
    public class AvailabilityStatus
    {
        /// <summary>
        /// Gets or sets a value indicating whether the availability status is disabled.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets the color associated with the availability status.
        /// </summary>
        public string? Color { get; set; }
    }
}

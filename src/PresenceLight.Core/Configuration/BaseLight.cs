namespace PresenceLight.Core
{
    /// <summary>
    /// Represents a base light configuration.
    /// </summary>
    public class BaseLight
    {
        /// <summary>
        /// Gets or sets a value indicating whether the light is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the selected item ID.
        /// </summary>
        public string? SelectedItemId { get; set; }

        /// <summary>
        /// Gets or sets the brightness level of the light.
        /// </summary>
        public int Brightness { get; set; }

        /// <summary>
        /// Gets or sets the presence light statuses.
        /// </summary>
        public PresenceLightStatuses Statuses { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use activity status for the light.
        /// </summary>
        public bool UseActivityStatus { get; set; }
    }
}

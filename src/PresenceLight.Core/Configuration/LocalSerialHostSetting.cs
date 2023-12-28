namespace PresenceLight.Core
{
    /// <summary>
    /// Represents the settings for a local serial host.
    /// </summary>
    public class LocalSerialHostSetting
    {
        /// <summary>
        /// Gets or sets the baud rate for the serial communication.
        /// </summary>
        public string? BaudRate { get; set; }

        /// <summary>
        /// Gets or sets the line ending characters for the serial communication.
        /// </summary>
        public string? LineEnding { get; set; }

        /// <summary>
        /// Gets or sets the port name for the serial communication.
        /// </summary>
        public string? Port { get; set; }

        /// <summary>
        /// Gets or sets the message to be sent over the serial communication.
        /// </summary>
        public string? Message { get; set; }
    }
}

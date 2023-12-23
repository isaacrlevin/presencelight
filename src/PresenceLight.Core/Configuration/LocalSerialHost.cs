namespace PresenceLight.Core
{
    /// <summary>
    /// Represents the configuration settings for a local serial host.
    /// </summary>
    public class LocalSerialHost : BaseLight
    {
        /// <summary>
        /// Gets or sets the main setup for the local serial host.
        /// </summary>
        public LocalSerialHostSetting LocalSerialHostMainSetup { get; set; }

        /// <summary>
        /// Gets or sets the available status for the local serial host.
        /// </summary>
        public string LocalSerialHostAvailable { get; set; }

        /// <summary>
        /// Gets or sets the busy status for the local serial host.
        /// </summary>
        public string LocalSerialHostBusy { get; set; }

        /// <summary>
        /// Gets or sets the be right back status for the local serial host.
        /// </summary>
        public string LocalSerialHostBeRightBack { get; set; }

        /// <summary>
        /// Gets or sets the away status for the local serial host.
        /// </summary>
        public string LocalSerialHostAway { get; set; }

        /// <summary>
        /// Gets or sets the do not disturb status for the local serial host.
        /// </summary>
        public string LocalSerialHostDoNotDisturb { get; set; }

        /// <summary>
        /// Gets or sets the available idle status for the local serial host.
        /// </summary>
        public string LocalSerialHostAvailableIdle { get; set; }

        /// <summary>
        /// Gets or sets the offline status for the local serial host.
        /// </summary>
        public string LocalSerialHostOffline { get; set; }

        /// <summary>
        /// Gets or sets the off status for the local serial host.
        /// </summary>
        public string LocalSerialHostOff { get; set; }

        /// <summary>
        /// Gets or sets the available activity status for the local serial host.
        /// </summary>
        public string LocalSerialHostActivityAvailable { get; set; }

        /// <summary>
        /// Gets or sets the in a call activity status for the local serial host.
        /// </summary>
        public string LocalSerialHostActivityInACall { get; set; }

        /// <summary>
        /// Gets or sets the in a conference call activity status for the local serial host.
        /// </summary>
        public string LocalSerialHostActivityInAConferenceCall { get; set; }

        /// <summary>
        /// Gets or sets the in a meeting activity status for the local serial host.
        /// </summary>
        public string LocalSerialHostActivityInAMeeting { get; set; }

        /// <summary>
        /// Gets or sets the presenting activity status for the local serial host.
        /// </summary>
        public string LocalSerialHostActivityPresenting { get; set; }

        /// <summary>
        /// Gets or sets the busy activity status for the local serial host.
        /// </summary>
        public string LocalSerialHostActivityBusy { get; set; }

        /// <summary>
        /// Gets or sets the away activity status for the local serial host.
        /// </summary>
        public string LocalSerialHostActivityAway { get; set; }

        /// <summary>
        /// Gets or sets the be right back activity status for the local serial host.
        /// </summary>
        public string LocalSerialHostActivityBeRightBack { get; set; }

        /// <summary>
        /// Gets or sets the do not disturb activity status for the local serial host.
        /// </summary>
        public string LocalSerialHostActivityDoNotDisturb { get; set; }

        /// <summary>
        /// Gets or sets the idle activity status for the local serial host.
        /// </summary>
        public string LocalSerialHostActivityIdle { get; set; }

        /// <summary>
        /// Gets or sets the offline activity status for the local serial host.
        /// </summary>
        public string LocalSerialHostActivityOffline { get; set; }

        /// <summary>
        /// Gets or sets the off activity status for the local serial host.
        /// </summary>
        public string LocalSerialHostActivityOff { get; set; }
    }
}

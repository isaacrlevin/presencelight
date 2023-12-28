namespace PresenceLight.Core
{
    /// <summary>
    /// Represents the configuration settings for a custom API.
    /// </summary>
    public class CustomApi : BaseLight
    {
        /// <summary>
        /// Gets or sets the timeout value for the custom API.
        /// </summary>
        public double CustomApiTimeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore certificate errors for the custom API.
        /// </summary>
        public bool IgnoreCertificateErrors { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use basic authentication for the custom API.
        /// </summary>
        public bool UseBasicAuth { get; set; }

        /// <summary>
        /// Gets or sets the username for basic authentication for the custom API.
        /// </summary>
        public string BasicAuthUserName { get; set; }

        /// <summary>
        /// Gets or sets the password for basic authentication for the custom API.
        /// </summary>
        public string BasicAuthUserPassword { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Available" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiAvailable { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Busy" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiBusy { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Be Right Back" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiBeRightBack { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Away" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiAway { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Do Not Disturb" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiDoNotDisturb { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Available Idle" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiAvailableIdle { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Offline" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiOffline { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Off" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiOff { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Activity: Available" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiActivityAvailable { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Activity: In a Call" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiActivityInACall { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Activity: In a Conference Call" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiActivityInAConferenceCall { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Activity: In a Meeting" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiActivityInAMeeting { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Activity: Presenting" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiActivityPresenting { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Activity: Busy" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiActivityBusy { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Activity: Away" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiActivityAway { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Activity: Be Right Back" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiActivityBeRightBack { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Activity: Do Not Disturb" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiActivityDoNotDisturb { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Activity: Idle" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiActivityIdle { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Activity: Offline" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiActivityOffline { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the "Activity: Off" status of the custom API.
        /// </summary>
        public CustomApiSetting CustomApiActivityOff { get; set; }
    }
}

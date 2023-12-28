using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace PresenceLight.Core
{
    /// <summary>
    /// Represents the statuses for availability and activity.
    /// </summary>
    public class PresenceLightStatuses
    {
        /// <summary>
        /// Gets or sets the availability status for "Available".
        /// </summary>
        public AvailabilityStatus AvailabilityAvailableStatus { get; set; }

        /// <summary>
        /// Gets or sets the availability status for "Available (Idle)".
        /// </summary>
        public AvailabilityStatus AvailabilityAvailableIdleStatus { get; set; }

        /// <summary>
        /// Gets or sets the availability status for "Away".
        /// </summary>
        public AvailabilityStatus AvailabilityAwayStatus { get; set; }

        /// <summary>
        /// Gets or sets the availability status for "Be Right Back".
        /// </summary>
        public AvailabilityStatus AvailabilityBeRightBackStatus { get; set; }

        /// <summary>
        /// Gets or sets the availability status for "Busy".
        /// </summary>
        public AvailabilityStatus AvailabilityBusyStatus { get; set; }

        /// <summary>
        /// Gets or sets the availability status for "Busy (Idle)".
        /// </summary>
        public AvailabilityStatus AvailabilityBusyIdleStatus { get; set; }

        /// <summary>
        /// Gets or sets the availability status for "Do Not Disturb".
        /// </summary>
        public AvailabilityStatus AvailabilityDoNotDisturbStatus { get; set; }

        /// <summary>
        /// Gets or sets the availability status for "Offline".
        /// </summary>
        public AvailabilityStatus AvailabilityOfflineStatus { get; set; }

        /// <summary>
        /// Gets or sets the availability status for "Presence Unknown".
        /// </summary>
        public AvailabilityStatus AvailabilityPresenceUnknownStatus { get; set; }

        /// <summary>
        /// Gets or sets the availability status for "Off".
        /// </summary>
        public AvailabilityStatus AvailabilityOffStatus { get; set; }

        /// <summary>
        /// Gets or sets the activity status for "Available".
        /// </summary>
        public AvailabilityStatus ActivityAvailableStatus { get; set; }

        /// <summary>
        /// Gets or sets the activity status for "Away".
        /// </summary>
        public AvailabilityStatus ActivityAwayStatus { get; set; }

        /// <summary>
        /// Gets or sets the activity status for "Be Right Back".
        /// </summary>
        public AvailabilityStatus ActivityBeRightBackStatus { get; set; }

        /// <summary>
        /// Gets or sets the activity status for "Busy".
        /// </summary>
        public AvailabilityStatus ActivityBusyStatus { get; set; }

        /// <summary>
        /// Gets or sets the activity status for "Do Not Disturb".
        /// </summary>
        public AvailabilityStatus ActivityDoNotDisturbStatus { get; set; }

        /// <summary>
        /// Gets or sets the activity status for "In a Call".
        /// </summary>
        public AvailabilityStatus ActivityInACallStatus { get; set; }

        /// <summary>
        /// Gets or sets the activity status for "In a Conference Call".
        /// </summary>
        public AvailabilityStatus ActivityInAConferenceCallStatus { get; set; }

        /// <summary>
        /// Gets or sets the activity status for "Inactive".
        /// </summary>
        public AvailabilityStatus ActivityInactiveStatus { get; set; }

        /// <summary>
        /// Gets or sets the activity status for "In a Meeting".
        /// </summary>
        public AvailabilityStatus ActivityInAMeetingStatus { get; set; }

        /// <summary>
        /// Gets or sets the activity status for "Offline".
        /// </summary>
        public AvailabilityStatus ActivityOfflineStatus { get; set; }

        /// <summary>
        /// Gets or sets the activity status for "Off".
        /// </summary>
        public AvailabilityStatus ActivityOffStatus { get; set; }

        /// <summary>
        /// Gets or sets the activity status for "Off Work".
        /// </summary>
        public AvailabilityStatus ActivityOffWorkStatus { get; set; }

        /// <summary>
        /// Gets or sets the activity status for "Out of Office".
        /// </summary>
        public AvailabilityStatus ActivityOutOfOfficeStatus { get; set; }

        /// <summary>
        /// Gets or sets the activity status for "Presence Unknown".
        /// </summary>
        public AvailabilityStatus ActivityPresenceUnknownStatus { get; set; }

        /// <summary>
        /// Gets or sets the activity status for "Presenting".
        /// </summary>
        public AvailabilityStatus ActivityPresentingStatus { get; set; }

        /// <summary>
        /// Gets or sets the activity status for "Urgent Interruptions Only".
        /// </summary>
        public AvailabilityStatus ActivityUrgentInterruptionsOnlyStatus { get; set; }
    }
}

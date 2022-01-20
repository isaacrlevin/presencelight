using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Microsoft.Graph.TermStore;

using Newtonsoft.Json;

namespace PresenceLight.Core
{
    public class Statuses
    {
        public AvailabilityStatus AvailabilityAvailableStatus { get; set; }

        public AvailabilityStatus AvailabilityAvailableIdleStatus { get; set; }

        public AvailabilityStatus AvailabilityAwayStatus { get; set; }

        public AvailabilityStatus AvailabilityBeRightBackStatus { get; set; }

        public AvailabilityStatus AvailabilityBusyStatus { get; set; }

        public AvailabilityStatus AvailabilityBusyIdleStatus { get; set; }

        public AvailabilityStatus AvailabilityDoNotDisturbStatus { get; set; }

        public AvailabilityStatus AvailabilityOfflineStatus { get; set; }

        public AvailabilityStatus AvailabilityPresenceUnknownStatus { get; set; }

        public AvailabilityStatus AvailabilityOffStatus { get; set; }

        public AvailabilityStatus ActivityAvailableStatus { get; set; }

        public AvailabilityStatus ActivityAwayStatus { get; set; }

        public AvailabilityStatus ActivityBeRightBackStatus { get; set; }

        public AvailabilityStatus ActivityBusyStatus { get; set; }

        public AvailabilityStatus ActivityDoNotDisturbStatus { get; set; }

        public AvailabilityStatus ActivityInACallStatus { get; set; }

        public AvailabilityStatus ActivityInAConferenceCallStatus { get; set; }

        public AvailabilityStatus ActivityInactiveStatus { get; set; }

        public AvailabilityStatus ActivityInAMeetingStatus { get; set; }

        public AvailabilityStatus ActivityOfflineStatus { get; set; }

        public AvailabilityStatus ActivityOffStatus { get; set; }

        public AvailabilityStatus ActivityOffWorkStatus { get; set; }

        public AvailabilityStatus ActivityOutOfOfficeStatus { get; set; }

        public AvailabilityStatus ActivityPresenceUnknownStatus { get; set; }

        public AvailabilityStatus ActivityPresentingStatus { get; set; }

        public AvailabilityStatus ActivityUrgentInterruptionsOnlyStatus { get; set; }
    }
}

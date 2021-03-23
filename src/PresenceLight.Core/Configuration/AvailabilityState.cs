using System.Collections.Generic;

namespace PresenceLight.Core
{
    public static class AvailabilityState
    {
        public const string None = "";
        public const string Available = "Available";
        public const string Busy = "Busy";
        public const string BeRightBack = "BeRightBack";
        public const string Away = "Away";
        public const string DoNotDisturb = "DoNotDisturb";
        public const string AvailableIdle = "AvailableIdle";
        public const string Offline = "Offline";
        public const string Off = "Off";

        public static IReadOnlyList<string> AllStates { get; } = new List<string>
            {
                None,
                Available,
                Busy,
                BeRightBack,
                Away,
                DoNotDisturb,
                AvailableIdle,
                Offline,
                Off,
            };
    }

    public static class ActivityState
    {
        public const string None = "";
        public const string Available = "Available";
        public const string Presenting = "Presenting";
        public const string InACall = "InACall";
        public const string InAMeeting = "InAMeeting";
        public const string Busy = "Busy";
        public const string Away = "Away";
        public const string BeRightBack = "BeRightBack";
        public const string DoNotDisturb = "DoNotDisturb";
        public const string Idle = "Idle";
        public const string Offline = "Offline";
        public const string Off = "Off";

        public static IReadOnlyList<string> AllStates { get; } = new List<string>
            {
                None,
                Available,
                Presenting,
                InACall,
                InAMeeting,
                Busy,
                Away,
                BeRightBack,
                DoNotDisturb,
                Idle,
                Offline,
                Off,
            };
    }
}

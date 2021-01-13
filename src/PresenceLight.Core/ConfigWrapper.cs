using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Microsoft.Graph.TermStore;

using Newtonsoft.Json;

namespace PresenceLight.Core
{
    public class LightSettings
    {
        public string HoursPassedStatus { get; set; }

        public bool SyncLights { get; set; }

        public string WorkingDays { get; set; }

        public bool UseWorkingHours { get; set; }

        public string WorkingHoursStartTime { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public DateTime? WorkingHoursStartTimeAsDate { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public DateTime? WorkingHoursEndTimeAsDate { get; set; }

        public string WorkingHoursEndTime { get; set; }

        public double PollingInterval { get; set; }

        public bool UseDefaultBrightness { get; set; }
        public int DefaultBrightness { get; set; }
        public Custom Custom { get; set; }

        public LIFX LIFX { get; set; }

        public Hue Hue { get; set; }

        public Yeelight Yeelight { get; set; }
    }

    public class Custom
    {
        public bool IsCustomApiEnabled { get; set; }

        public string? SelectedCustomLightId { get; set; }

        public string? CustomApiAvailableMethod { get; set; }

        public string? CustomApiAvailableUri { get; set; }

        public string? CustomApiBusyMethod { get; set; }

        public string? CustomApiBusyUri { get; set; }

        public string? CustomApiBeRightBackMethod { get; set; }

        public string? CustomApiBeRightBackUri { get; set; }

        public string? CustomApiAwayMethod { get; set; }

        public string? CustomApiAwayUri { get; set; }

        public string? CustomApiDoNotDisturbMethod { get; set; }

        public string? CustomApiDoNotDisturbUri { get; set; }

        public string? CustomApiAvailableIdleMethod { get; set; }

        public string? CustomApiAvailableIdleUri { get; set; }

        public string? CustomApiOfflineMethod { get; set; }

        public string? CustomApiOfflineUri { get; set; }

        public string? CustomApiOffMethod { get; set; }

        public string? CustomApiOffUri { get; set; }

        public string? CustomApiActivityAvailableMethod { get; set; }

        public string? CustomApiActivityAvailableUri { get; set; }

        public string? CustomApiActivityInACallMethod { get; set; }

        public string? CustomApiActivityInACallUri { get; set; }

        public string? CustomApiActivityInAMeetingMethod { get; set; }

        public string? CustomApiActivityInAMeetingUri { get; set; }

        public string? CustomApiActivityPresentingMethod { get; set; }

        public string? CustomApiActivityPresentingUri { get; set; }

        public string? CustomApiActivityBusyMethod { get; set; }

        public string? CustomApiActivityBusyUri { get; set; }

        public string? CustomApiActivityAwayMethod { get; set; }

        public string? CustomApiActivityAwayUri { get; set; }

        public string? CustomApiActivityBeRightBackMethod { get; set; }

        public string? CustomApiActivityBeRightBackUri { get; set; }

        public string? CustomApiActivityDoNotDisturbMethod { get; set; }

        public string? CustomApiActivityDoNotDisturbUri { get; set; }

        public string? CustomApiActivityIdleMethod { get; set; }

        public string? CustomApiActivityIdleUri { get; set; }

        public string? CustomApiActivityOfflineMethod { get; set; }

        public string? CustomApiActivityOfflineUri { get; set; }

        public string? CustomApiActivityOffMethod { get; set; }

        public string? CustomApiActivityOffUri { get; set; }

        public double CustomApiTimeout { get; set; }
    }

    public class LIFX
    {

        public string? LIFXApiKey { get; set; }

        public string? LIFXClientId { get; set; }

        public string? LIFXClientSecret { get; set; }

        public bool IsLIFXEnabled { get; set; }

        public string? SelectedLIFXItemId { get; set; }

        public int LIFXBrightness { get; set; }

        public AvailabilityStatus AvailableStatus { get; set; }

        public AvailabilityStatus BusyStatus { get; set; }

        public AvailabilityStatus AwayStatus { get; set; }

        public AvailabilityStatus BeRightBackStatus { get; set; }

        public AvailabilityStatus DoNotDisturbStatus { get; set; }

        public AvailabilityStatus OfflineStatus { get; set; }

        public AvailabilityStatus OffStatus { get; set; }
    }

    public class Hue
    {
        public string? RemoteHueClientId { get; set; }

        public string? RemoteHueClientAppName { get; set; }

        public string? RemoteHueClientSecret { get; set; }
        public string? HueApiKey { get; set; }

        public string? SelectedHueLightId { get; set; }

        public int HueBrightness { get; set; }

        [Required]
        [RegularExpression(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b",
        ErrorMessage = "Not a valid IP Address")]
        public string? HueIpAddress { get; set; }

        public bool IsPhillipsHueEnabled { get; set; }

        public AvailabilityStatus AvailableStatus { get; set; }

        public AvailabilityStatus BusyStatus { get; set; }

        public AvailabilityStatus AwayStatus { get; set; }

        public AvailabilityStatus BeRightBackStatus { get; set; }

        public AvailabilityStatus DoNotDisturbStatus { get; set; }

        public AvailabilityStatus OfflineStatus { get; set; }

        public AvailabilityStatus OffStatus { get; set; }
        public bool UseRemoteApi { get; set; }

        public string RemoteBridgeId { get; set; }
    }

    public class AvailabilityStatus
    {
        public Boolean Disabled { get; set; }

        public string? Colour { get; set; }

    }

    public class Yeelight
    {
        public string? SelectedYeelightId { get; set; }

        public int YeelightBrightness { get; set; }

        public bool IsYeelightEnabled { get; set; }

        public AvailabilityStatus AvailableStatus { get; set; }

        public AvailabilityStatus BusyStatus { get; set; }

        public AvailabilityStatus AwayStatus { get; set; }

        public AvailabilityStatus BeRightBackStatus { get; set; }

        public AvailabilityStatus DoNotDisturbStatus { get; set; }

        public AvailabilityStatus OfflineStatus { get; set; }

        public AvailabilityStatus OffStatus { get; set; }
    }

    public class AADSettings
    {
        public string? ClientId { get; set; }

        public string? Instance { get; set; }

        public string? RedirectUri { get; set; }
    }

    public class BaseConfig
    {
        public bool SendDiagnosticData { get; set; }
        public string? IconType { get; set; }
        public LightSettings LightSettings { get; set; }
    }
}

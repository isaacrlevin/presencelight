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
        public CustomApi CustomApi { get; set; }

        public LIFX LIFX { get; set; }

        public Hue Hue { get; set; }

        public Yeelight Yeelight { get; set; }
    }

    public class CustomApi
    {
        public bool IsCustomApiEnabled { get; set; }

        public string? SelectedCustomLightId { get; set; }

        public double CustomApiTimeout { get; set; }

        public CustomApiSetting CustomApiAvailable { get; set; }


        public CustomApiSetting CustomApiBusy { get; set; }


        public CustomApiSetting CustomApiBeRightBack { get; set; }


        public CustomApiSetting CustomApiAway { get; set; }


        public CustomApiSetting CustomApiDoNotDisturb { get; set; }


        public CustomApiSetting CustomApiAvailableIdle { get; set; }


        public CustomApiSetting CustomApiOffline { get; set; }


        public CustomApiSetting CustomApiOff { get; set; }


        public CustomApiSetting CustomApiActivityAvailable { get; set; }


        public CustomApiSetting CustomApiActivityInACall { get; set; }


        public CustomApiSetting CustomApiActivityInAMeeting { get; set; }


        public CustomApiSetting CustomApiActivityPresenting { get; set; }

        public CustomApiSetting CustomApiActivityBusy { get; set; }


        public CustomApiSetting CustomApiActivityAway { get; set; }


        public CustomApiSetting CustomApiActivityBeRightBack { get; set; }


        public CustomApiSetting CustomApiActivityDoNotDisturb { get; set; }


        public CustomApiSetting CustomApiActivityIdle { get; set; }


        public CustomApiSetting CustomApiActivityOffline { get; set; }


        public CustomApiSetting CustomApiActivityOff { get; set; }
    }

    public class LIFX
    {
        public string? LIFXApiKey { get; set; }

        public string? LIFXClientId { get; set; }

        public string? LIFXClientSecret { get; set; }

        public bool IsLIFXEnabled { get; set; }

        public string? SelectedLIFXItemId { get; set; }

        public int LIFXBrightness { get; set; }

        public Statuses Statuses { get; set; }
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

        public Statuses Statuses { get; set; }
        public bool UseRemoteApi { get; set; }

        public string RemoteBridgeId { get; set; }
    }

    public class CustomApiSetting
    {
        public string? Method { get; set; }
        public string? Uri { get; set; }
    }

    public class Yeelight
    {
        public string? SelectedYeelightId { get; set; }

        public int YeelightBrightness { get; set; }

        public bool IsYeelightEnabled { get; set; }

        public Statuses Statuses { get; set; }
    }

    public class Statuses
    {
        public AvailabilityStatus AvailabilityAvailableStatus { get; set; }

        public AvailabilityStatus AvailabilityBusyStatus { get; set; }

        public AvailabilityStatus AvailabilityAwayStatus { get; set; }

        public AvailabilityStatus AvailabilityBeRightBackStatus { get; set; }

        public AvailabilityStatus AvailabilityDoNotDisturbStatus { get; set; }

        public AvailabilityStatus AvailabilityOfflineStatus { get; set; }

        public AvailabilityStatus AvailabilityOffStatus { get; set; }

    }

    public class AvailabilityStatus
    {
        public Boolean Disabled { get; set; }

        public string? Colour { get; set; }

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

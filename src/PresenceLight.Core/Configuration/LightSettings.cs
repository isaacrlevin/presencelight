using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Microsoft.Graph.TermStore;

using Newtonsoft.Json;

namespace PresenceLight.Core
{
    public class BaseLight
    {
        public bool IsEnabled { get; set; }

        public string? SelectedItemId { get; set; }

        public int Brightness { get; set; }

        public Statuses Statuses { get; set; }

        public bool UseActivityStatus { get; set; }
    }

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

    public class CustomApiSetting
    {
        public string? Method { get; set; }
        public string? Uri { get; set; }
    }
}

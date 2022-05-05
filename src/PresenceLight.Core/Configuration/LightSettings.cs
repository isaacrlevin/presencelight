using System;

using Newtonsoft.Json;

namespace PresenceLight.Core
{

    public class LightSettings
    {
        public string HoursPassedStatus { get; set; }

        public bool SyncLights { get; set; }

        public string WorkingDays { get; set; }

        public bool UseWorkingHours { get; set; }

        public bool UseAmPm { get; set; }

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

        public LocalSerialHost LocalSerialHost { get; set; }

        public LIFX LIFX { get; set; }

        public Hue Hue { get; set; }

        public Yeelight Yeelight { get; set; }

        public Wiz Wiz { get; set; }
    }
}

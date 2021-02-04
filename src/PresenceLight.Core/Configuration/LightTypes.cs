using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Microsoft.Graph.TermStore;

using Newtonsoft.Json;

namespace PresenceLight.Core
{
    public class CustomApi : BaseLight
    {
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

    public class LIFX : BaseLight
    {
        public string? LIFXApiKey { get; set; }

        public string? LIFXClientId { get; set; }

        public string? LIFXClientSecret { get; set; }
    }

    public class Hue : BaseLight
    {
        public string? RemoteHueClientId { get; set; }

        public string? RemoteHueClientAppName { get; set; }

        public string? RemoteHueClientSecret { get; set; }
        public string? HueApiKey { get; set; }

        [Required]
        [RegularExpression(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b",
        ErrorMessage = "Not a valid IP Address")]
        public string? HueIpAddress { get; set; }

        public bool UseRemoteApi { get; set; }

        public string RemoteBridgeId { get; set; }
    }

    public class Yeelight : BaseLight
    {
    }
}

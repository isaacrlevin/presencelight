using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Microsoft.Graph.TermStore;

using Newtonsoft.Json;

namespace PresenceLight.Core
{
    public class BaseConfig
    {
        public string Theme { get; set; }
        public bool SendDiagnosticData { get; set; }
        public bool StartMinimized { get; set; }
        public string? IconType { get; set; }

        public string LogLevel { get; set; }

        public bool LogInfo { get => (string.IsNullOrEmpty(LogLevel) || LogLevel == "None" || LogLevel == "Error" ) ? false : true; }

        public bool LogError { get => (string.IsNullOrEmpty(LogLevel) || LogLevel == "None") ? false : true; }

        public LightSettings LightSettings { get; set; }
    }
}

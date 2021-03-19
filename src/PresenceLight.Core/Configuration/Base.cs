using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Microsoft.Graph.TermStore;

using Newtonsoft.Json;

namespace PresenceLight.Core
{
    public class BaseConfig
    {
        public bool SendDiagnosticData { get; set; }
        public bool StartMinimized { get; set; }
        public string? IconType { get; set; }
        public LightSettings LightSettings { get; set; }
    }
}

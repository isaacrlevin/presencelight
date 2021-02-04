using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Microsoft.Graph.TermStore;

using Newtonsoft.Json;

namespace PresenceLight.Core
{
    public class AADSettings
    {
        public string? ClientId { get; set; }

        public string? Instance { get; set; }

        public string? RedirectUri { get; set; }
    }

}

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace PresenceLight.Core
{
    /// <summary>
    /// Represents the base configuration for the application.
    /// </summary>
    public class BaseConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether to send diagnostic data.
        /// </summary>
        public bool SendDiagnosticData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the application should start minimized.
        /// </summary>
        public bool StartMinimized { get; set; }

        /// <summary>
        /// Gets or sets the type of icon to be used.
        /// </summary>
        public string? IconType { get; set; }

        /// <summary>
        /// Gets or sets the light settings for the application.
        /// </summary>
        public LightSettings LightSettings { get; set; }

        /// <summary>
        /// Gets or sets the type of application.
        /// </summary>
        public string AppType { get; set; }
    }
}

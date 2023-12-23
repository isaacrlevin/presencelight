using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


using Newtonsoft.Json;

namespace PresenceLight.Core
{
    /// <summary>
    /// Represents the Azure Active Directory (AAD) settings.
    /// </summary>
    public class AADSettings
    {
        /// <summary>
        /// Gets or sets the client ID for AAD authentication.
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Gets or sets the AAD instance URL.
        /// </summary>
        public string? Instance { get; set; }

        /// <summary>
        /// Gets or sets the redirect URI for AAD authentication.
        /// </summary>
        public string? RedirectUri { get; set; }
    }
}

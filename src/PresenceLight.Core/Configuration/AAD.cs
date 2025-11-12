using System;
using System.Collections.Generic;
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
        /// Gets or sets the tenant ID for AAD authentication.
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the Client Secret for AAD authentication.
        /// </summary>
        public string? ClientSecret { get; set; }
        /// <summary>
        /// Gets or sets the AAD instance URL.
        /// </summary>
        public string? Instance { get; set; }

        /// <summary>
        /// Gets or sets the redirect URI for AAD authentication.
        /// </summary>
        public string? RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the redirect host for AAD authentication.
        /// </summary>
        public string? RedirectHost { get; set; }

        /// <summary>
        /// Gets or sets the CallbackPath for AAD authentication.
        /// </summary>
        public string? CallbackPath { get; set; }

        /// <summary>
        /// Gets or sets the Scopes for AAD authentication.
        /// </summary>
        public List<string>? Scopes { get; set; }
    }
}

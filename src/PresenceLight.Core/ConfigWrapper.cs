using System.ComponentModel.DataAnnotations;

namespace PresenceLight.Core
{
    public class ConfigWrapper
    {
        public string ClientId { get; set; }

        public string LifxApiKey { get; set; }

        public string ClientSecret { get; set; }

        public string Domain { get; set; }

        public string Instance { get; set; }
        public string TenantId { get; set; }

        public string RedirectUri { get; set; }
        public string CallbackPath { get; set; }
        public string HueApiKey { get; set; }

        public string Authority => $"https://login.microsoftonline.com/{TenantId}";

        public string ApiScopes { get; set; }

        public string SelectedHueLightId { get; set; }

        public string SelectedLifxItemId { get; set; }

        [Required]
        [RegularExpression(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b",
        ErrorMessage = "Characters are not allowed.")]
        public string HueIpAddress { get; set; }
        public string IconType { get; set; }

        public bool IsLifxEnabled { get; set; }

        public bool IsPhillipsEnabled { get; set; }
    }
}

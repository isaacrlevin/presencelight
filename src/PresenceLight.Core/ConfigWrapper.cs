using System;
using System.ComponentModel.DataAnnotations;

namespace PresenceLight.Core
{
    public class ConfigWrapper
    {
        public string? ClientId { get; set; }

        public string? LIFXApiKey { get; set; }

        public string? Instance { get; set; }

        public string? RedirectUri { get; set; }

        public string? CallbackPath { get; set; }
        public string? HueApiKey { get; set; }

        public string? ApiScopes { get; set; }

        public double PollingInterval { get; set; }

        public int Brightness { get; set; }

        public string? SelectedHueLightId { get; set; }

        public string? SelectedLIFXItemId { get; set; }

        public string? SelectedYeeLightId { get; set; }

        [Required]
        [RegularExpression(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b",
        ErrorMessage = "Characters are not allowed.")]
        public string? HueIpAddress { get; set; }
        public string? IconType { get; set; }

        public bool IsLIFXEnabled { get; set; }

        public bool IsPhillipsEnabled { get; set; }

        public bool IsYeelightEnabled { get; set; }

        public bool IsCustomApiEnabled { get; set; }

        public string? CustomApiAvailableMethod { get; set; }

        public string? CustomApiAvailableUri { get; set; }

        public string? CustomApiBusyMethod { get; set; }

        public string? CustomApiBusyUri { get; set; }

        public string? CustomApiBeRightBackMethod { get; set; }

        public string? CustomApiBeRightBackUri { get; set; }

        public string? CustomApiAwayMethod { get; set; }

        public string? CustomApiAwayUri { get; set; }

        public string? CustomApiDoNotDisturbMethod { get; set; }

        public string? CustomApiDoNotDisturbUri { get; set; }

        public string? CustomApiOfflineMethod { get; set; }

        public string? CustomApiOfflineUri { get; set; }

        public string? CustomApiOffMethod { get; set; }

        public string? CustomApiOffUri { get; set; }

        public double CustomApiTimeout { get; set; }
    }
}

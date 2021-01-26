using System;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Graph;
using Microsoft.Extensions.Logging;

namespace PresenceLight.Core
{
    public interface ICustomApiService
    {
        Task<string> SetColor(string availability, string? activity);
    }

    public class CustomApiService : ICustomApiService
    {
        private string _currentAvailability = "";
        private string _currentActivity = "";

        static readonly HttpClient client = new HttpClient
        {
            // TODO: Make this configurable
            Timeout = TimeSpan.FromSeconds(10)
        };

        private readonly ILogger<CustomApiService> _logger;
        private readonly BaseConfig _options;

        public CustomApiService(IOptionsMonitor<BaseConfig> optionsAccessor, ILogger<CustomApiService> logger)
        {
            _logger = logger;
            _options = optionsAccessor.CurrentValue;
        }

        public CustomApiService(BaseConfig options)
        {
            _options = options;
        }

        public async Task<string> SetColor(string availability, string? activity)
        {
            string result = await SetAvailability(availability);
            result += await SetActivity(activity);
            return result;
        }

        private async Task<string> CallCustomApiForActivityChanged(object sender, string newActivity)
        {
            string method = string.Empty;
            string uri = string.Empty;
            string result = string.Empty;

            switch (newActivity)
            {
                case "Available":
                    method = _options.LightSettings.Custom.CustomApiActivityAvailableMethod;
                    uri = _options.LightSettings.Custom.CustomApiActivityAvailableUri;
                    break;
                case "Presenting":
                    method = _options.LightSettings.Custom.CustomApiActivityPresentingMethod;
                    uri = _options.LightSettings.Custom.CustomApiActivityPresentingUri;
                    break;
                case "InACall":
                    method = _options.LightSettings.Custom.CustomApiActivityInACallMethod;
                    uri = _options.LightSettings.Custom.CustomApiActivityInACallUri;
                    break;
                case "InAMeeting":
                    method = _options.LightSettings.Custom.CustomApiActivityInAMeetingMethod;
                    uri = _options.LightSettings.Custom.CustomApiActivityInAMeetingUri;
                    break;
                case "Busy":
                    method = _options.LightSettings.Custom.CustomApiActivityBusyMethod;
                    uri = _options.LightSettings.Custom.CustomApiActivityBusyUri;
                    break;
                case "Away":
                    method = _options.LightSettings.Custom.CustomApiActivityAwayMethod;
                    uri = _options.LightSettings.Custom.CustomApiActivityAwayUri;
                    break;
                case "BeRightBack":
                    method = _options.LightSettings.Custom.CustomApiActivityBeRightBackMethod;
                    uri = _options.LightSettings.Custom.CustomApiActivityBeRightBackUri;
                    break;
                case "DoNotDisturb":
                    method = _options.LightSettings.Custom.CustomApiActivityDoNotDisturbMethod;
                    uri = _options.LightSettings.Custom.CustomApiActivityDoNotDisturbUri;
                    break;
                case "Idle":
                    method = _options.LightSettings.Custom.CustomApiActivityIdleMethod;
                    uri = _options.LightSettings.Custom.CustomApiActivityIdleUri;
                    break;
                case "Offline":
                    method = _options.LightSettings.Custom.CustomApiActivityOfflineMethod;
                    uri = _options.LightSettings.Custom.CustomApiActivityOfflineUri;
                    break;
                case "Off":
                    method = _options.LightSettings.Custom.CustomApiActivityOffMethod;
                    uri = _options.LightSettings.Custom.CustomApiActivityOffUri;
                    break;
                default:
                    break;
            }

            return await PerformWebRequest(method, uri, result);
        }

        private async Task<string> CallCustomApiForAvailabilityChanged(object sender, string newAvailability)
        {
            string method = string.Empty;
            string uri = string.Empty;
            string result = string.Empty;

            switch (newAvailability)
            {
                case "Available":
                    method = _options.LightSettings.Custom.CustomApiAvailableMethod;
                    uri = _options.LightSettings.Custom.CustomApiAvailableUri;
                    break;
                case "Busy":
                    method = _options.LightSettings.Custom.CustomApiBusyMethod;
                    uri = _options.LightSettings.Custom.CustomApiBusyUri;
                    break;
                case "BeRightBack":
                    method = _options.LightSettings.Custom.CustomApiBeRightBackMethod;
                    uri = _options.LightSettings.Custom.CustomApiBeRightBackUri;
                    break;
                case "Away":
                    method = _options.LightSettings.Custom.CustomApiAwayMethod;
                    uri = _options.LightSettings.Custom.CustomApiAwayUri;
                    break;
                case "DoNotDisturb":
                    method = _options.LightSettings.Custom.CustomApiDoNotDisturbMethod;
                    uri = _options.LightSettings.Custom.CustomApiDoNotDisturbUri;
                    break;
                case "AvailableIdle":
                    method = _options.LightSettings.Custom.CustomApiAvailableIdleMethod;
                    uri = _options.LightSettings.Custom.CustomApiAvailableIdleUri;
                    break;
                case "Offline":
                    method = _options.LightSettings.Custom.CustomApiOfflineMethod;
                    uri = _options.LightSettings.Custom.CustomApiOfflineUri;
                    break;
                case "Off":
                    method = _options.LightSettings.Custom.CustomApiOffMethod;
                    uri = _options.LightSettings.Custom.CustomApiOffUri;
                    break;
                default:
                    break;
            }

            return await PerformWebRequest(method, uri, result);
        }

        private async Task<string> SetAvailability(string availability)
        {
            string result = "";
            if (!string.IsNullOrEmpty(availability) && availability != _currentAvailability)
            {
                _currentAvailability = availability;
                result = await CallCustomApiForAvailabilityChanged(this, availability);
            }
            return result;
        }

        private async Task<string> SetActivity(string activity)
        {
            string result = "";
            if (!string.IsNullOrEmpty(activity) && activity != _currentActivity)
            {
                _currentActivity = activity;
                result = await CallCustomApiForActivityChanged(this, activity);
            }
            return result;
        }

        private async Task<string> PerformWebRequest(string method, string uri, string result)
        {
            if (!string.IsNullOrEmpty(method) && !string.IsNullOrEmpty(uri))
            {
                try
                {
                    HttpResponseMessage response = new HttpResponseMessage();
                    switch (method)
                    {
                        case "GET":
                            response = await client.GetAsync(uri);
                            break;
                        case "POST":
                            response = await client.PostAsync(uri, null);
                            break;
                    }


                    string responseBody = await response.Content.ReadAsStringAsync();
                    result = $"{(int)response.StatusCode} {response.StatusCode}: {responseBody}";
                    string message = $"Sending {method} methd to {uri}";
                    Helpers.AppendLogger(_logger, message);
                }
                catch (Exception e)
                {
                    Helpers.AppendLogger(_logger, "Error Performing Web Request", e);
                    result = $"Error: {e.Message}";
                }
            }

            return result;
        }
    }
}

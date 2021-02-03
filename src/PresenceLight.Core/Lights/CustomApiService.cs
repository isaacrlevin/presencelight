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
        void Initialize(BaseConfig options);
    }

    public class CustomApiService : ICustomApiService
    {
        private IWorkingHoursService _workingHoursService;
        private string _currentAvailability = string.Empty;
        private string _currentActivity = string.Empty;

        static readonly HttpClient client = new HttpClient
        {
            // TODO: Make this configurable
            Timeout = TimeSpan.FromSeconds(10)
        };

        private readonly ILogger<CustomApiService> _logger;
        private BaseConfig _options;

        public CustomApiService(IOptionsMonitor<BaseConfig> optionsAccessor, ILogger<CustomApiService> logger, IWorkingHoursService workingHoursService)
        {
            _logger = logger;
            _options = optionsAccessor.CurrentValue;
            _workingHoursService = workingHoursService;
        }

        public void Initialize(BaseConfig options)
        {
            _options = options;
        }

        public async Task<string> SetColor(string availability, string? activity)
        {
            if (this._workingHoursService.UseWorkingHours
                && !this._workingHoursService.IsInWorkingHours)
            {
                // If we are outside of working hours we should signal that we are off
                availability = activity = "Off";
            }

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
                    method = _options.LightSettings.Custom.CustomApiActivityAvailable.Method;
                    uri = _options.LightSettings.Custom.CustomApiActivityAvailable.Uri;
                    break;
                case "Presenting":
                    method = _options.LightSettings.Custom.CustomApiActivityPresenting.Method;
                    uri = _options.LightSettings.Custom.CustomApiActivityPresenting.Uri;
                    break;
                case "InACall":
                    method = _options.LightSettings.Custom.CustomApiActivityInACall.Method;
                    uri = _options.LightSettings.Custom.CustomApiActivityInACall.Uri;
                    break;
                case "InAMeeting":
                    method = _options.LightSettings.Custom.CustomApiActivityInAMeeting.Method;
                    uri = _options.LightSettings.Custom.CustomApiActivityInAMeeting.Uri;
                    break;
                case "Busy":
                    method = _options.LightSettings.Custom.CustomApiActivityBusy.Method;
                    uri = _options.LightSettings.Custom.CustomApiActivityBusy.Uri;
                    break;
                case "Away":
                    method = _options.LightSettings.Custom.CustomApiActivityAway.Method;
                    uri = _options.LightSettings.Custom.CustomApiActivityAway.Uri;
                    break;
                case "BeRightBack":
                    method = _options.LightSettings.Custom.CustomApiActivityBeRightBack.Method;
                    uri = _options.LightSettings.Custom.CustomApiActivityBeRightBack.Uri;
                    break;
                case "DoNotDisturb":
                    method = _options.LightSettings.Custom.CustomApiActivityDoNotDisturb.Method;
                    uri = _options.LightSettings.Custom.CustomApiActivityDoNotDisturb.Uri;
                    break;
                case "Idle":
                    method = _options.LightSettings.Custom.CustomApiActivityIdle.Method;
                    uri = _options.LightSettings.Custom.CustomApiActivityIdle.Uri;
                    break;
                case "Offline":
                    method = _options.LightSettings.Custom.CustomApiActivityOffline.Method;
                    uri = _options.LightSettings.Custom.CustomApiActivityOffline.Uri;
                    break;
                case "Off":
                    method = _options.LightSettings.Custom.CustomApiActivityOff.Method;
                    uri = _options.LightSettings.Custom.CustomApiActivityOff.Uri;
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
                    method = _options.LightSettings.Custom.CustomApiAvailable.Method;
                    uri = _options.LightSettings.Custom.CustomApiAvailable.Uri;
                    break;
                case "Busy":
                    method = _options.LightSettings.Custom.CustomApiBusy.Method;
                    uri = _options.LightSettings.Custom.CustomApiBusy.Uri;
                    break;
                case "BeRightBack":
                    method = _options.LightSettings.Custom.CustomApiBeRightBack.Method;
                    uri = _options.LightSettings.Custom.CustomApiBeRightBack.Uri;
                    break;
                case "Away":
                    method = _options.LightSettings.Custom.CustomApiAway.Method;
                    uri = _options.LightSettings.Custom.CustomApiAway.Uri;
                    break;
                case "DoNotDisturb":
                    method = _options.LightSettings.Custom.CustomApiDoNotDisturb.Method;
                    uri = _options.LightSettings.Custom.CustomApiDoNotDisturb.Uri;
                    break;
                case "AvailableIdle":
                    method = _options.LightSettings.Custom.CustomApiAvailableIdle.Method;
                    uri = _options.LightSettings.Custom.CustomApiAvailableIdle.Uri;
                    break;
                case "Offline":
                    method = _options.LightSettings.Custom.CustomApiOffline.Method;
                    uri = _options.LightSettings.Custom.CustomApiOffline.Uri;
                    break;
                case "Off":
                    method = _options.LightSettings.Custom.CustomApiOff.Method;
                    uri = _options.LightSettings.Custom.CustomApiOff.Uri;
                    break;
                default:
                    break;
            }

            return await PerformWebRequest(method, uri, result);
        }

        private async Task<string> SetAvailability(string availability)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(availability) && availability != _currentAvailability)
            {
                _currentAvailability = availability;
                result = await CallCustomApiForAvailabilityChanged(this, availability);
            }
            return result;
        }

        private async Task<string> SetActivity(string activity)
        {
            string result = string.Empty;
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

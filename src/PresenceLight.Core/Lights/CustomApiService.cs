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
            if (!_workingHoursService.UseWorkingHours || (_workingHoursService.UseWorkingHours && _workingHoursService.IsInWorkingHours))
            {
                // If we are outside of working hours we should signal that we are off
                availability = activity = availability;
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
                    method = _options.LightSettings.CustomApi.CustomApiActivityAvailable.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiActivityAvailable.Uri;
                    break;
                case "Presenting":
                    method = _options.LightSettings.CustomApi.CustomApiActivityPresenting.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiActivityPresenting.Uri;
                    break;
                case "InACall":
                    method = _options.LightSettings.CustomApi.CustomApiActivityInACall.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiActivityInACall.Uri;
                    break;
                case "InAMeeting":
                    method = _options.LightSettings.CustomApi.CustomApiActivityInAMeeting.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiActivityInAMeeting.Uri;
                    break;
                case "Busy":
                    method = _options.LightSettings.CustomApi.CustomApiActivityBusy.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiActivityBusy.Uri;
                    break;
                case "Away":
                    method = _options.LightSettings.CustomApi.CustomApiActivityAway.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiActivityAway.Uri;
                    break;
                case "BeRightBack":
                    method = _options.LightSettings.CustomApi.CustomApiActivityBeRightBack.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiActivityBeRightBack.Uri;
                    break;
                case "DoNotDisturb":
                    method = _options.LightSettings.CustomApi.CustomApiActivityDoNotDisturb.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiActivityDoNotDisturb.Uri;
                    break;
                case "Idle":
                    method = _options.LightSettings.CustomApi.CustomApiActivityIdle.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiActivityIdle.Uri;
                    break;
                case "Offline":
                    method = _options.LightSettings.CustomApi.CustomApiActivityOffline.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiActivityOffline.Uri;
                    break;
                case "Off":
                    method = _options.LightSettings.CustomApi.CustomApiActivityOff.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiActivityOff.Uri;
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
                    method = _options.LightSettings.CustomApi.CustomApiAvailable.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiAvailable.Uri;
                    break;
                case "Busy":
                    method = _options.LightSettings.CustomApi.CustomApiBusy.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiBusy.Uri;
                    break;
                case "BeRightBack":
                    method = _options.LightSettings.CustomApi.CustomApiBeRightBack.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiBeRightBack.Uri;
                    break;
                case "Away":
                    method = _options.LightSettings.CustomApi.CustomApiAway.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiAway.Uri;
                    break;
                case "DoNotDisturb":
                    method = _options.LightSettings.CustomApi.CustomApiDoNotDisturb.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiDoNotDisturb.Uri;
                    break;
                case "AvailableIdle":
                    method = _options.LightSettings.CustomApi.CustomApiAvailableIdle.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiAvailableIdle.Uri;
                    break;
                case "Offline":
                    method = _options.LightSettings.CustomApi.CustomApiOffline.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiOffline.Uri;
                    break;
                case "Off":
                    method = _options.LightSettings.CustomApi.CustomApiOff.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiOff.Uri;
                    break;
                default:
                    break;
            }

            return await PerformWebRequest(method, uri, result);
        }

        private async Task<string> SetAvailability(string availability)
        {
            string result = await CallCustomApiForAvailabilityChanged(this, availability);
            return result;
        }

        private async Task<string> SetActivity(string activity)
        {
            string result = await CallCustomApiForActivityChanged(this, activity);
            return result;
        }

        private async Task<string> PerformWebRequest(string method, string uri, string result)
        {
            using (Serilog.Context.LogContext.PushProperty("method", method))
            using (Serilog.Context.LogContext.PushProperty("uri", uri))
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
                        string message = $"Sending {method} method to {uri}";

                        _logger.LogInformation(message);

                        using (Serilog.Context.LogContext.PushProperty("result", result))
                            _logger.LogDebug(message + " Results");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error Performing Web Request");
                        result = $"Error: {e.Message}";
                    }
                }

                return result;
            }
        }
    }
}

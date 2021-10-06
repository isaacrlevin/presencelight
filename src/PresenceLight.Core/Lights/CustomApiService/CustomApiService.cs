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
        Task<string> SetColor(string availability, string? activity, CancellationToken cancellationToken = default);
        void Initialize(BaseConfig options);
    }


    public class CustomApiService : ICustomApiService
    {
        private MediatR.IMediator _mediator;
        private string _currentAvailability = string.Empty;
        private string _currentActivity = string.Empty;

        readonly HttpClient _client;

        private readonly ILogger<CustomApiService> _logger;
        private BaseConfig _options;

        public CustomApiService(IOptionsMonitor<BaseConfig> optionsAccessor, ILogger<CustomApiService> logger, MediatR.IMediator mediator)
        {
            _logger = logger;
            _options = optionsAccessor.CurrentValue;
            _mediator = mediator;

            _client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(optionsAccessor.CurrentValue.LightSettings.CustomApi.CustomApiTimeout > 0 ?
                                               optionsAccessor.CurrentValue.LightSettings.CustomApi.CustomApiTimeout :
                                               20)
            };
        }

        public void Initialize(BaseConfig options)
        {
            _options = options;
        }

        public async Task<string> SetColor(string availability, string? activity, CancellationToken cancellationToken = default)
        {
            string result = await SetAvailability(availability, cancellationToken);
            result += await SetActivity(activity, cancellationToken);
            return result;
        }

        private async Task<string> CallCustomApiForActivityChanged(object sender, string newActivity, CancellationToken cancellationToken)
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
                case "InAConferenceCall":
                    method = _options.LightSettings.CustomApi.CustomApiActivityInAConferenceCall.Method;
                    uri = _options.LightSettings.CustomApi.CustomApiActivityInAConferenceCall.Uri;
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

            return await PerformWebRequest(method, uri, result, cancellationToken);
        }

        private async Task<string> CallCustomApiForAvailabilityChanged(object sender, string newAvailability, CancellationToken cancellationToken)
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

            return await PerformWebRequest(method, uri, result, cancellationToken);
        }

        private async Task<string> SetAvailability(string availability, CancellationToken cancellationToken)
        {
            string result = string.Empty;
            if (availability != _currentAvailability)
            {
                result = await CallCustomApiForAvailabilityChanged(this, availability, cancellationToken);
                if (!cancellationToken.IsCancellationRequested)
                {
                    _currentAvailability = availability;
                }
                else
                {
                    // operation was cancelled
                }
                    
            }
            else
            {
                // availability did not change: don't spam call the api
            }
            return result;
        }

        private async Task<string> SetActivity(string activity, CancellationToken cancellationToken)
        {
            string result = string.Empty;
            if (activity != _currentActivity)
            {
                result = await CallCustomApiForActivityChanged(this, activity, cancellationToken);
                if (!cancellationToken.IsCancellationRequested)
                {
                    _currentActivity = activity;
                }
                else
                {
                    // operation was cancelled
                }
            }
            else
            {
                // activity did not change: don't spam call the api
            }
            return result;
        }

        static Stack<string> _lastUriCalled = new Stack<string>(1);
        private async Task<string> PerformWebRequest(string method, string uri, string result, CancellationToken cancellationToken)
        {
            if (_lastUriCalled.Contains($"{method}|{uri}"))
            {
                _logger.LogDebug("No Change to State... NOT calling Api");
                return "Skipped";
            }


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
                                response = await _client.GetAsync(uri, cancellationToken);
                                break;
                            case "POST":
                                response = await _client.PostAsync(uri, null, cancellationToken);
                                break;
                        }


                        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                        result = $"{(int)response.StatusCode} {response.StatusCode}: {responseBody}";
                        string message = $"Sending {method} method to {uri}";

                        _logger.LogInformation(message);
                        _lastUriCalled.TryPop(out string res);
                        _lastUriCalled.Push($"{method}|{uri}");

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

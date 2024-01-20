using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace PresenceLight.Core
{
    public interface ICustomApiService
    {
        Task<string> SetColor(string availability, string? activity, CancellationToken cancellationToken = default);
        void Initialize(AppState _appState);
    }


    public class CustomApiService : ICustomApiService
    {
        private MediatR.IMediator _mediator;
        private string _currentAvailability = string.Empty;
        private string _currentActivity = string.Empty;

        HttpClient _client;

        private readonly ILogger<CustomApiService> _logger;
        private AppState _appState;

        public CustomApiService(AppState appState, ILogger<CustomApiService> logger, MediatR.IMediator mediator)
        {
            _logger = logger;
            _appState = appState;
            _mediator = mediator;

            _client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(_appState.Config.LightSettings.CustomApi.CustomApiTimeout > 0 ?
                                                   _appState.Config.LightSettings.CustomApi.CustomApiTimeout :
                                                   20)
            };
        }

        public void Initialize(AppState appState)
        {
            _appState = appState;
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
            string body = string.Empty;
            string result = string.Empty;

            switch (newActivity)
            {
                case "Available":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiActivityAvailable.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiActivityAvailable.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiActivityAvailable.Body;
                    break;
                case "Presenting":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiActivityPresenting.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiActivityPresenting.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiActivityPresenting.Body;
                    break;
                case "InACall":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiActivityInACall.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiActivityInACall.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiActivityInACall.Body;
                    break;
                case "InAConferenceCall":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiActivityInAConferenceCall.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiActivityInAConferenceCall.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiActivityInAConferenceCall.Body;
                    break;
                case "InAMeeting":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiActivityInAMeeting.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiActivityInAMeeting.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiActivityInAMeeting.Body;
                    break;
                case "Busy":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiActivityBusy.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiActivityBusy.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiActivityBusy.Body;
                    break;
                case "Away":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiActivityAway.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiActivityAway.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiActivityAway.Body;
                    break;
                case "BeRightBack":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiActivityBeRightBack.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiActivityBeRightBack.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiActivityBeRightBack.Body;
                    break;
                case "DoNotDisturb":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiActivityDoNotDisturb.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiActivityDoNotDisturb.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiActivityDoNotDisturb.Body;
                    break;
                case "Idle":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiActivityIdle.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiActivityIdle.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiActivityIdle.Body;
                    break;
                case "Offline":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiActivityOffline.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiActivityOffline.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiActivityOffline.Body;
                    break;
                case "Off":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiActivityOff.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiActivityOff.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiActivityOff.Body;
                    break;
                default:
                    break;
            }

            return await PerformWebRequest(method, uri, body, result, cancellationToken);
        }

        private async Task<string> CallCustomApiForAvailabilityChanged(object sender, string newAvailability, CancellationToken cancellationToken)
        {
            string method = string.Empty;
            string uri = string.Empty;
            string body = string.Empty;
            string result = string.Empty;

            switch (newAvailability)
            {
                case "Available":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiAvailable.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiAvailable.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiAvailable.Body;
                    break;
                case "Busy":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiBusy.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiBusy.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiBusy.Body;
                    break;
                case "BeRightBack":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiBeRightBack.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiBeRightBack.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiBeRightBack.Body;
                    break;
                case "Away":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiAway.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiAway.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiAway.Body;
                    break;
                case "DoNotDisturb":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiDoNotDisturb.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiDoNotDisturb.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiDoNotDisturb.Body;
                    break;
                case "AvailableIdle":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiAvailableIdle.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiAvailableIdle.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiAvailableIdle.Body;
                    break;
                case "Offline":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiOffline.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiOffline.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiOffline.Body;
                    break;
                case "Off":
                    method = _appState.Config.LightSettings.CustomApi.CustomApiOff.Method;
                    uri = _appState.Config.LightSettings.CustomApi.CustomApiOff.Uri;
                    body = _appState.Config.LightSettings.CustomApi.CustomApiOff.Body;
                    break;
                default:
                    break;
            }

            return await PerformWebRequest(method, uri, body, result, cancellationToken);
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
        private async Task<string> PerformWebRequest(string method, string uri, string body, string result, CancellationToken cancellationToken)
        {
            if (_lastUriCalled.Contains($"{method}|{uri}"))
            {
                _logger.LogDebug("No Change to State... NOT calling Api");
                return "Skipped";
            }


            using (Serilog.Context.LogContext.PushProperty("method", method))
            using (Serilog.Context.LogContext.PushProperty("uri", uri))
            using (Serilog.Context.LogContext.PushProperty("body", body))
            {
                if (Helpers.AreStringsNotEmpty(new string[] { method, uri }))
                {
                    try
                    {
                        if (_appState.Config.LightSettings.CustomApi.IgnoreCertificateErrors)
                        {
                            var httpClientHandler = new HttpClientHandler();
                            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                            _client = new HttpClient(httpClientHandler);
                        }
                        else
                        {
                            _client = new HttpClient();
                        }

                        _client.Timeout = TimeSpan.FromSeconds(_appState.Config.LightSettings.CustomApi.CustomApiTimeout > 0 ?
                                                           _appState.Config.LightSettings.CustomApi.CustomApiTimeout :
                                                           20);

                        HttpResponseMessage response = new HttpResponseMessage();

                        if (_appState.Config.LightSettings.CustomApi.UseBasicAuth)
                        {
                            var byteArray = Encoding.ASCII.GetBytes($"{_appState.Config.LightSettings.CustomApi.BasicAuthUserName}:{_appState.Config.LightSettings.CustomApi.BasicAuthUserPassword}");

                            _client.DefaultRequestHeaders.Authorization = new
                            AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                        }

                        switch (method)
                        {
                            case "GET":
                                response = await _client.GetAsync(uri, cancellationToken);
                                break;
                            case "POST":
                                var content = new StringContent(body, Encoding.UTF8, "application/json");
                                response = await _client.PostAsync(uri, content, cancellationToken);
                                break;
                        }


                        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                        result = $"{(int)response.StatusCode} {response.StatusCode}: {responseBody}";
                        string message = $"Sending {method} method to {uri} with body {body}";

                        _logger.LogInformation(message);
                        _lastUriCalled.TryPop(out string res);
                        _lastUriCalled.Push($"{method}|{uri}|{body}");

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

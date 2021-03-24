using System;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using MediatR;
using PresenceLight.Core.PubSub;

namespace PresenceLight.Core
{
    public interface ICustomApiService
    {
        Task<CustomApiResponse> SetColor(string availability, string? activity, CancellationToken cancellationToken = default);
    }

    public class CustomApiService : ICustomApiService, INotificationHandler<InitializeNotification>
    {
        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        private string _lastCallId = string.Empty;

        readonly HttpClient _client;

        private readonly ILogger<CustomApiService> _logger;
        private BaseConfig _options;

        public CustomApiService(IOptionsMonitor<BaseConfig> optionsAccessor, ILogger<CustomApiService> logger)
        {
            _logger = logger;
            _options = optionsAccessor.CurrentValue;

            _client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(optionsAccessor.CurrentValue.LightSettings.CustomApi.CustomApiTimeout > 0 ?
                                               optionsAccessor.CurrentValue.LightSettings.CustomApi.CustomApiTimeout :
                                               20)
            };
        }

        public Task Handle(InitializeNotification notification, CancellationToken cancellationToken)
        {
            _options = notification.Config;
            return Task.CompletedTask;
        }

        public async Task<CustomApiResponse> SetColor(string availability, string? activity, CancellationToken cancellationToken = default)
        {
            try
            {
                await _semaphoreSlim.WaitAsync(cancellationToken);

                // If we are outside of working hours we should signal that we are off
                // availability = activity = availability;

                CustomApiSubscription setting = FindSetting(availability, activity);
                if (setting == null)
                {
                    return CustomApiResponse.None;
                }

                return await PerformWebRequest(setting.Method, setting.Uri, cancellationToken);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private CustomApiSubscription? FindSetting(string availability, string? activity)
        {
            // Try to find exact match
            CustomApiSubscription setting = FindValidSetting(s => s.Availability == availability && s.Activity == activity);
            if (setting != null)
            {
                return setting;
            }

            // Try to find exact activity
            setting = FindValidSetting(s => s.Activity == activity);
            if (setting != null)
            {
                return setting;
            }

            // Try to find exact availability
            setting = FindValidSetting(s => s.Availability == availability);
            if (setting != null)
            {
                return setting;
            }

            // Try to find first default
            setting = FindValidSetting(s => string.IsNullOrWhiteSpace(s.Availability) && string.IsNullOrWhiteSpace(s.Activity));
            if (setting != null)
            {
                return setting;
            }

            return null;
        }

        private CustomApiSubscription? FindValidSetting(Predicate<CustomApiSubscription> predicate)
        {
            Predicate<CustomApiSubscription> validatedPredicate = s => predicate(s) && s.IsValid();
            return _options.LightSettings.CustomApi.Subscriptions.FirstOrDefault(s => validatedPredicate(s));
        }

        private async Task<CustomApiResponse> PerformWebRequest(string method, string uri, CancellationToken cancellationToken)
        {
            string callId = $"{method}|{uri}";
            if (_lastCallId == callId)
            {
                return CustomApiResponse.None;
            }

            using (Serilog.Context.LogContext.PushProperty("method", method))
            using (Serilog.Context.LogContext.PushProperty("uri", uri))
            {
                if (!string.IsNullOrEmpty(method) && !string.IsNullOrEmpty(uri))
                {
                    try
                    {
                        HttpResponseMessage response = new HttpResponseMessage();

                        var httpMethod = new HttpMethod(method);
                        var request = new HttpRequestMessage(httpMethod, uri);

                        string message = $"Sending {method} method to {uri}";
                        _logger.LogInformation(message);

                        response = await _client.SendAsync(request, cancellationToken);

                        CustomApiResponse apiResponse = await CustomApiResponse.CreateAsync(method, uri, response, cancellationToken);

                        using (Serilog.Context.LogContext.PushProperty("result", apiResponse.ToString()))
                        {
                            _logger.LogDebug(message + " Results");
                        }

                        if (apiResponse.IsSuccessful)
                        {
                            _lastCallId = callId;
                        }

                        return apiResponse;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error Performing Web Request");
                        return CustomApiResponse.Create(method, uri, e);
                    }
                }
            }

            return CustomApiResponse.None;
        }
    }
}

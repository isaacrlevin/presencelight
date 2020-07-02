using System;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Graph;

namespace PresenceLight.Core
{
    public interface ICustomApiService
    {
        Task<string> SetColor(string availability);
    }

    public class CustomApiService : ICustomApiService
    {
        static readonly HttpClient client = new HttpClient
        {
            // TODO: Make this configurable
            Timeout = TimeSpan.FromSeconds(10)
        };
        
        private readonly ConfigWrapper _options;

        public CustomApiService(IOptionsMonitor<ConfigWrapper> optionsAccessor)
        {
            _options = optionsAccessor.CurrentValue;
        }

        public CustomApiService(ConfigWrapper options)
        {
            _options = options;
        }

        public async Task<string> SetColor(string availability)
        {
            string method = string.Empty;
            string uri = string.Empty;
            string result = string.Empty;

            switch (availability)
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

            if (method != string.Empty && uri != string.Empty)
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
                }
                catch (Exception e)
                {
                    result = $"Error: {e.Message}";
                }
            }

            return result;
        }
    }
}

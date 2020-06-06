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
                    method = _options.CustomApiAvailableMethod;
                    uri = _options.CustomApiAvailableUri;
                    break;
                case "Busy":
                    method = _options.CustomApiBusyMethod;
                    uri = _options.CustomApiBusyUri;
                    break;
                case "BeRightBack":
                    method = _options.CustomApiBeRightBackMethod;
                    uri = _options.CustomApiBeRightBackUri;
                    break;
                case "Away":
                    method = _options.CustomApiAwayMethod;
                    uri = _options.CustomApiAwayUri;
                    break;
                case "DoNotDisturb":
                    method = _options.CustomApiDoNotDisturbMethod;
                    uri = _options.CustomApiDoNotDisturbUri;
                    break;
                case "Offline":
                    method = _options.CustomApiOfflineMethod;
                    uri = _options.CustomApiOfflineUri;
                    break;
                case "Off":
                    method = _options.CustomApiOffMethod;
                    uri = _options.CustomApiOffUri;
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

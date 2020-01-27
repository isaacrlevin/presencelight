using Microsoft.Graph;
using Microsoft.Identity.Client;
using PresenceLight.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace PresenceLight.Core.Graph
{
    public interface IGraphService
    {
        GraphServiceClient GetAuthenticatedGraphClient();
    }

    public class GraphService : IGraphService
    {
        private readonly ConfigWrapper _options;

        public GraphService(IOptionsMonitor<ConfigWrapper> optionsAccessor)
        {
            _options = optionsAccessor.CurrentValue;
        }
        public GraphServiceClient GetAuthenticatedGraphClient()
        {
            var authenticationProvider = CreateAuthorizationProvider();
            var _graphServiceClient = new GraphServiceClient(authenticationProvider);
            return _graphServiceClient;
        }

        private HttpClient GetAuthenticatedHTTPClient()
        {
            var authenticationProvider = CreateAuthorizationProvider();
            var _httpClient = new HttpClient(new AuthHandler(authenticationProvider, new HttpClientHandler()));
            return _httpClient;
        }

        private IAuthenticationProvider CreateAuthorizationProvider()
        {
            var clientId = _options.ApplicationId;
            var redirectUri = _options.RedirectUri;
            var authority = $"https://login.microsoftonline.com/{_options.TenantId}";

            List<string> scopes = new List<string>
            {
                "https://graph.microsoft.com/.default"
            };

            var pca = PublicClientApplicationBuilder.Create(clientId)
                                                    .WithAuthority(authority)
                                                    .WithRedirectUri(redirectUri)
                                                    .Build();
            return new DeviceCodeFlowAuthorizationProvider(pca, scopes);
        }
    }
}
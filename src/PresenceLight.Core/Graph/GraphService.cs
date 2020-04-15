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
        GraphServiceClient GetAuthenticatedGraphClient(Type t);
    }

    public class GraphService : IGraphService
    {
        private readonly ConfigWrapper _options;

        public GraphService(IOptionsMonitor<ConfigWrapper> optionsAccessor)
        {
            _options = optionsAccessor.CurrentValue;
        }
        public GraphServiceClient GetAuthenticatedGraphClient(Type t)
        {
            var authenticationProvider = CreateAuthorizationProvider(t);// CreateAuthorizationProvider();
            var _graphServiceClient = new GraphServiceClient(authenticationProvider);
            return _graphServiceClient;
        }

        private IAuthenticationProvider CreateAuthorizationProvider(Type t)
        {
            var clientId = _options.ClientId;
            var redirectUri = _options.RedirectUri;
            var authority = $"https://login.microsoftonline.com/{_options.TenantId}";

            List<string> scopes = new List<string>
            {
                "https://graph.microsoft.com/.default"
            };

            var pca = PublicClientApplicationBuilder.Create(clientId)
                                                    .WithAuthority(authority)
                                                    //.WithRedirectUri(redirectUri)
                                                    .WithRedirectUri("http://localhost")
                                                    .Build();

            TokenCacheHelper.EnableSerialization(pca.UserTokenCache);

            return (IAuthenticationProvider)Activator.CreateInstance(t, new object[] { pca, scopes });
        }
    }
}
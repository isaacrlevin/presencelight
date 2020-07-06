using System;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

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
            List<string> scopes = new List<string>
            {
                "https://graph.microsoft.com/.default"
            };

            var pca = PublicClientApplicationBuilder.Create(_options.ClientId)
                                                    .WithAuthority($"{_options.Instance}common/")
                                                    .WithRedirectUri(_options.RedirectUri)
                                                    .Build();

            TokenCacheHelper.EnableSerialization(pca.UserTokenCache);

            return (IAuthenticationProvider)Activator.CreateInstance(t, new object[] { pca, scopes });
        }
    }
}

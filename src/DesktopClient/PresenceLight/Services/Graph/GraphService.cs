using System;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using PresenceLight.Core;

namespace PresenceLight.Graph
{
    public interface IGraphService
    {
        GraphServiceClient GetAuthenticatedGraphClient();
    }

    public class GraphService : IGraphService
    {
        AADSettings aadSettings;

        public GraphService(IOptions<AADSettings> optionsAccessor)
        {
            aadSettings = optionsAccessor.Value;
        }
        public GraphServiceClient GetAuthenticatedGraphClient()
        {
            var authenticationProvider = CreateAuthorizationProvider();
            var _graphServiceClient = new GraphServiceClient(authenticationProvider);
            return _graphServiceClient;
        }

        private IAuthenticationProvider CreateAuthorizationProvider()
        {
            List<string> scopes = new List<string>
            {
                "https://graph.microsoft.com/.default"
            };

            var pca = PublicClientApplicationBuilder.Create(aadSettings.ClientId)
                                                    .WithAuthority($"{aadSettings.Instance}common/")
                                                    .WithRedirectUri(aadSettings.RedirectUri)
                                                    .Build();

            TokenCacheHelper.EnableSerialization(pca.UserTokenCache);

            return new WPFAuthorizationProvider(pca, scopes);
        }
    }
}

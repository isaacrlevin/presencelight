using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace PresenceLight.Core
{
    public class AuthorizationProvider : IAuthenticationProvider
    {
        public IPublicClientApplication PubClient { get; set; }
        public IConfidentialClientApplication ConfClient { get; set; }
        BaseConfig config;
        public IAccount UserAccount { get; set; }

        public AuthorizationProvider(IOptions<BaseConfig> optionsAccessor)
        {
            config = optionsAccessor.Value;

            if (config.AppType == "Desktop")
            {
                PubClient = PublicClientApplicationBuilder.Create(config.AADSettings.ClientId)
                                                        .WithAuthority($"{config.AADSettings.Instance}common/")
                                                        .WithRedirectUri(config.AADSettings.RedirectUri)
                                                        .Build();

                TokenCacheHelper.EnableSerialization(PubClient.UserTokenCache);
            }
            else if (config.AppType == "Web")
            {
                if (optionsAccessor != null && optionsAccessor.Value != null)
                {
                    config = optionsAccessor.Value;
                    if (!Helpers.AreStringsNotEmpty(new string[] { config.AADSettings.ClientId,
                                                        config.AADSettings.ClientSecret,
                                                        config.AADSettings.Instance,
                                                        config.AADSettings.RedirectHost,
                                                        config.AADSettings.CallbackPath }))
                    { }
                    else
                    {
                        ConfClient = ConfidentialClientApplicationBuilder
                         .Create(config.AADSettings.ClientId)
                         .WithClientSecret(config.AADSettings.ClientSecret)
                         .WithAuthority($"{config.AADSettings.Instance}common/v2.0")
                         .WithRedirectUri($"{config.AADSettings.RedirectHost}{config.AADSettings.CallbackPath}")
                         .Build();

                        var cacheHelper = CreateCacheHelperAsync(config.AADSettings.ClientId);
                        cacheHelper.RegisterCache(ConfClient.UserTokenCache);
                    }
                }
            }
        }

        public async Task<string> AcquireToken()
        {
            AuthenticationResult authResult = null;
            string accessToken = null;

            if (config.AppType == "Desktop")
            {
                var accounts = await PubClient.GetAccountsAsync();
                var firstAccount = accounts.FirstOrDefault();

                try
                {
                    authResult = await PubClient.AcquireTokenSilent(config.AADSettings.Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();

                    UserAccount = authResult.Account;
                    accessToken = authResult.AccessToken;
                }
                catch (MsalUiRequiredException)
                {
                    try
                    {
                        authResult = await PubClient.AcquireTokenInteractive(config.AADSettings.Scopes)
                           .WithUseEmbeddedWebView(false)
                           .ExecuteAsync();

                        UserAccount = authResult.Account;
                        accessToken = authResult.AccessToken;
                    }
                    catch
                    {

                    }
                }
            }
            else if (config.AppType == "Web")
            {

                try
                {
                    var result = await ConfClient
                        .AcquireTokenSilent(config.AADSettings.Scopes, UserAccount)
                        .ExecuteAsync();

                    UserAccount = result.Account;
                    accessToken = result.AccessToken;
                }
                catch (System.Exception)
                { }
            }

            return accessToken;
        }

        private static MsalCacheHelper CreateCacheHelperAsync(string clientId)
        {
            StorageCreationProperties storageProperties;

            try
            {
                storageProperties =
                    new StorageCreationPropertiesBuilder(
                    "cache.plaintext",
                    System.AppContext.BaseDirectory,
                    clientId)
                    .WithLinuxUnprotectedFile()
                    .Build();

                var cacheHelper = MsalCacheHelper.CreateAsync(storageProperties).Result;
                return cacheHelper;

            }
            catch (MsalCachePersistenceException e)
            {
                storageProperties =
                    new StorageCreationPropertiesBuilder(
                    "cache.plaintext",
                    System.AppContext.BaseDirectory,
                    clientId)
                    .WithLinuxUnprotectedFile()
                    .Build();

                var cacheHelper = MsalCacheHelper.CreateAsync(storageProperties).Result;

                cacheHelper.VerifyPersistence();

                return cacheHelper;
            }
        }

        async Task IAuthenticationProvider.AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext, CancellationToken cancellationToken)
        {
            string accessToken = await AcquireToken();

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Add("Authorization", $"Bearer {accessToken}");
            }
        }
    }
}


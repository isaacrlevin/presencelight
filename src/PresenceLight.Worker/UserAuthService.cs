using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using PresenceLight.Core;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace PresenceLight.Worker
{
    public class UserAuthService : IAuthenticationProvider
    {
        static string[] graphScopes = { "https://graph.microsoft.com/.default" };
        private readonly IConfidentialClientApplication _msalClient;
        private IAccount _userAccount;

        public UserAuthService(IConfiguration configuration)
        {
            var config = new ConfigWrapper();
            configuration.Bind(config);

            _msalClient = ConfidentialClientApplicationBuilder
                .Create(config.ClientId)
                .WithClientSecret(config.ClientSecret)
                .WithAuthority($"{config.Instance}common/v2.0")
                .WithRedirectUri(config.RedirectUri)
                .Build();

            // TODO: Token cache serialization
            // For now, the cache will be maintained in memory
        }

        public async Task<bool> IsUserAuthenticated()
        {
            // If we already have the user account we're
            // authenticated
            if (null != _userAccount)
            {
                return true;
            }

            // See if there are any accounts in the cache
            var accounts = await _msalClient.GetAccountsAsync();

            _userAccount = accounts.FirstOrDefault();
            return null != _userAccount;
        }

        public async Task<string> AddUserToTokenCache(string authorizationCode)
        {
            var result = await _msalClient
                .AcquireTokenByAuthorizationCode(graphScopes, authorizationCode)
                .ExecuteAsync();

            _userAccount = result.Account;

            return result.IdToken;
        }

        public async Task<string> GetAccessToken()
        {
            if (null == _userAccount)
            {
                return null;
            }

            try
            {
                var result = await _msalClient
                    .AcquireTokenSilent(graphScopes, _userAccount)
                    .ExecuteAsync();

                return result.AccessToken;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public async Task SignOut()
        {
            if (null != _userAccount)
            {
                await _msalClient.RemoveAsync(_userAccount);
                _userAccount = null;
            }
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage requestMessage)
        {
            var accessToken = await GetAccessToken();
            if (!string.IsNullOrEmpty(accessToken))
            {
                requestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }
    }
}

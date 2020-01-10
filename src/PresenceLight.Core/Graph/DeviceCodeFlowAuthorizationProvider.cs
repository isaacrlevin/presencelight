using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PresenceLight.Core.Helpers
{
    public class DeviceCodeFlowAuthorizationProvider : IAuthenticationProvider
    {
        private readonly IPublicClientApplication _application;
        private readonly List<string> _scopes;
        private string _authToken;
        private AuthenticationResult _authResult;
        private IAccount _account;
        public DeviceCodeFlowAuthorizationProvider(IPublicClientApplication application, List<string> scopes)
        {
            _application = application;
            _scopes = scopes;
        }
        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                var result = await _application.AcquireTokenWithDeviceCode(_scopes, callback =>
                {
                    Console.WriteLine(callback.Message);
                    return Task.FromResult(0);
                }).ExecuteAsync();
                _authResult = result;
                _authToken = result.AccessToken;
                var accounts = await _application.GetAccountsAsync();
                _account = accounts.FirstOrDefault();
            }

            if (_authResult .ExpiresOn.ToLocalTime() <= DateTime.Now)
            {
                var result = await _application.AcquireTokenSilent(_scopes, _account).ExecuteAsync();

                _authResult = result;
                _authToken = result.AccessToken;
                var accounts = await _application.GetAccountsAsync();
                _account = accounts.FirstOrDefault();
            }
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", _authToken);
        }
    }
}

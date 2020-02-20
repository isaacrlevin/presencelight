using MSGraph = Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;

namespace PresenceLight.Core.Helpers
{
    public class WPFAuthorizationProvider : MSGraph.IAuthenticationProvider
    {
        private readonly IPublicClientApplication _application;
        private readonly List<string> _scopes;
        string graphAPIEndpoint = "https://graph.microsoft.com/beta/me/presence";

        public WPFAuthorizationProvider(IPublicClientApplication application, List<string> scopes)
        {
            _application = application;
            _scopes = scopes;
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            AuthenticationResult authResult = null;

            var accounts = await _application.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();

            try
            {
                authResult = await _application.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
                .ExecuteAsync();

            }
            catch (MsalUiRequiredException)
            {
                try
                {

                    authResult = await _application.AcquireTokenInteractive(_scopes)
                       .WithParentActivityOrWindow(new WindowInteropHelper(Application.Current.MainWindow).Handle)
                       .ExecuteAsync();
                }
                catch
                {

                }
            }

            if (authResult != null)
            {
                //request.Headers.Authorization = new AuthenticationHeaderValue("bearer", _authToken);
                await GetHttpContentWithToken(graphAPIEndpoint, authResult.AccessToken);
            }
        }

        public async Task<string> GetHttpContentWithToken(string url, string token)
        {
            var httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response;
            try
            {
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
                //Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}

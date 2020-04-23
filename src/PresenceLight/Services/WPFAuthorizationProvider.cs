using MSGraph = Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Net.Http.Headers;
using PresenceLight.Telemetry;

namespace PresenceLight.Core.Helpers
{
    public class WPFAuthorizationProvider : MSGraph.IAuthenticationProvider
    {
        public static IPublicClientApplication _application;
        private readonly List<string> _scopes;

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
                    await Application.Current.Dispatcher.Invoke(async () =>
                     {
                         authResult = await _application.AcquireTokenInteractive(_scopes)
                            .WithUseEmbeddedWebView(false)
                            .ExecuteAsync();
                     });
                }
                catch (Exception ex)
                {

                }
            }

            if (authResult != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("bearer", authResult.AccessToken);
            }
        }
    }
}

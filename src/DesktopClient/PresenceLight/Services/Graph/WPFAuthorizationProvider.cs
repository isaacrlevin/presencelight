using System;
using MSGraph = Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using KiotaAuth = Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions;
using System.Threading;

namespace PresenceLight.Graph
{
    public class WPFAuthorizationProvider : KiotaAuth.IAuthenticationProvider
    {
        public static IPublicClientApplication Application;
        private readonly List<string> _scopes;

        public WPFAuthorizationProvider(IPublicClientApplication application, List<string> scopes)
        {
            Application = application;
            _scopes = scopes;
        }

        async Task IAuthenticationProvider.AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext, CancellationToken cancellationToken)
        {
            AuthenticationResult authResult = null;

            var accounts = await Application.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();

            try
            {
                authResult = await Application.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
                .ExecuteAsync(cancellationToken);

            }
            catch (MsalUiRequiredException)
            {
                try
                {
                    await System.Windows.Application.Current.Dispatcher.Invoke<Task>(async () =>
                     {
                         authResult = await Application.AcquireTokenInteractive(_scopes)
                            .WithUseEmbeddedWebView(false)
                            .ExecuteAsync();
                     });
                }
                catch 
                {

                }
            }

            if (authResult != null)
            {
                request.Headers.Add("Authorization", $"Bearer {authResult.AccessToken}");                
            }
        }
    }
}

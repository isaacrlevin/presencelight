using Microsoft.Graph;
using Microsoft.Identity.Client;
using PresenceLight.Core.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
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
                    string[] parts = callback.Message.Split(' ');
                    var code = parts[Array.FindIndex(parts, a => a.Trim() == "code") + 1];
                    TextCopy.Clipboard.SetText(code);
                    OpenBrowser("https://www.microsoft.com/devicelogin");
                    Console.WriteLine(callback.Message);
                    return Task.FromResult(0);
                }).ExecuteAsync();
                _authResult = result;
                _authToken = result.AccessToken;
                var accounts = await _application.GetAccountsAsync();
                _account = accounts.FirstOrDefault();
            }

            if (_authResult.ExpiresOn.ToLocalTime() <= DateTime.Now)
            {
                var result = await _application.AcquireTokenSilent(_scopes, _account).ExecuteAsync();

                _authResult = result;
                _authToken = result.AccessToken;
                var accounts = await _application.GetAccountsAsync();
                _account = accounts.FirstOrDefault();
            }
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", _authToken);
        }

        private void OpenBrowser(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    System.Diagnostics.Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    System.Diagnostics.Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    System.Diagnostics.Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}

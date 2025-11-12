using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace PresenceLight.Core
{
    public class LoginService
    {
        public GraphServiceClient GraphServiceClient { get; set; }
        
        private readonly AuthorizationProvider _authProvider;
        private readonly ILogger<LoginService> _logger;
        private readonly IOptionsMonitor<BaseConfig> _configMonitor;
        private readonly IDisposable? _reloadSubscription;
        private readonly AppState _appState;
        private readonly object _sync = new();
        public bool IsInitialized { get; set; }
        private BaseConfig config;

        public LoginService(IOptionsMonitor<BaseConfig> configMonitor, AuthorizationProvider authProvider, AppState appState, ILogger<LoginService> logger)
        {
            _authProvider = authProvider;
            _appState = appState;
            _logger = logger;
            _configMonitor = configMonitor;
            config = _configMonitor.CurrentValue;
            _appState.AadConfigComplete = authProvider.RebuildMsalClients();

            _reloadSubscription = _configMonitor.OnChange(async newConfig =>
            {
                try
                {
                    if (AuthorizationProvider.AadChanged(config, newConfig))
                    {
                        _logger?.LogInformation("AAD settings changed; signing out and invalidating auth provider.");
                        _appState.SignOutRequested = true;
                        await SignOut();
                        _authProvider.Invalidate();
                        _appState.AadConfigComplete = authProvider.RebuildMsalClients();
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Error reacting to AAD settings change.");
                }

                config = newConfig;
            });
        }

        public async Task GetAuthenticatedGraphClient()
        {
            await _authProvider.AcquireToken();
            GraphServiceClient = new GraphServiceClient(_authProvider);
            IsInitialized = true;
        }

        public async Task<bool> IsUserAuthenticated()
        {
            BaseConfig config = _configMonitor.CurrentValue;
            // If we already have the user account we're
            // authenticated
            if (_authProvider.UserAccount != null)
            {
                return true;
            }

            if (config.AppType == "Desktop")
            {
                if (_authProvider.PubClient == null)
                {
                    return false;
                }

                var accounts = await _authProvider.PubClient.GetAccountsAsync();

                _authProvider.UserAccount = accounts.FirstOrDefault();
                return null != _authProvider.UserAccount;
            }
            else if (config.AppType == "Web")
            {
                if (_authProvider.ConfClient == null)
                {
                    return false;
                }

                var accounts = await _authProvider.ConfClient.GetAccountsAsync();

                _authProvider.UserAccount = accounts.FirstOrDefault();
                return null != _authProvider.UserAccount;
            }
            return false;
        }

        public async Task<string> AddUserToTokenCache(string authorizationCode)
        {
            BaseConfig config = _configMonitor.CurrentValue;
            var result = await _authProvider.ConfClient
                .AcquireTokenByAuthorizationCode(config.AADSettings.Scopes, authorizationCode)
                .ExecuteAsync();

            _authProvider.UserAccount = result.Account;

            return result.IdToken;
        }

        public async Task SignOut()
        {
            BaseConfig config = _configMonitor.CurrentValue;
            if (config.AppType == "Desktop")
            {
                if (_authProvider.UserAccount != null)
                {
                    await _authProvider.PubClient.RemoveAsync(_authProvider.UserAccount);
                }
            }
            else if (config.AppType == "Web")
            {
                if (_authProvider.UserAccount != null)
                {
                    await _authProvider.ConfClient.RemoveAsync(_authProvider.UserAccount);
                }
            }
            _authProvider.UserAccount = null;
            IsInitialized = false;
            GraphServiceClient = null;
        }
    }
}

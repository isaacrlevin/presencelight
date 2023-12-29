using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace PresenceLight.Core
{
    public class LoginService
    {
        public GraphServiceClient GraphServiceClient { get; set; }
        BaseConfig config;
        AuthorizationProvider authProvider;

        public bool IsInitialized { get; set; }

        public LoginService(IOptions<BaseConfig> optionsAccessor, AuthorizationProvider _authProvider)
        {
            config = optionsAccessor.Value;
            authProvider = _authProvider;
        }
        public async Task GetAuthenticatedGraphClient()
        {
            await authProvider.AcquireToken();
            GraphServiceClient = new GraphServiceClient(authProvider);
            IsInitialized = true;
        }

        public async Task<bool> IsUserAuthenticated()
        {
            // If we already have the user account we're
            // authenticated
            if (authProvider.UserAccount != null)
            {
                return true;
            }

            if (config.AppType == "Desktop")
            {
                if (authProvider.PubClient == null)
                {
                    return false;
                }

                var accounts = await authProvider.PubClient.GetAccountsAsync();

                authProvider.UserAccount = accounts.FirstOrDefault();
                return null != authProvider.UserAccount;
            }
            else if (config.AppType == "Web")
            {
                if (authProvider.ConfClient == null)
                {
                    return false;
                }

                var accounts = await authProvider.ConfClient.GetAccountsAsync();

                authProvider.UserAccount = accounts.FirstOrDefault();
                return null != authProvider.UserAccount;
            }
            return false;
        }

        public async Task<string> AddUserToTokenCache(string authorizationCode)
        {
            var result = await authProvider.ConfClient
                .AcquireTokenByAuthorizationCode(config.AADSettings.Scopes, authorizationCode)
                .ExecuteAsync();

            authProvider.UserAccount = result.Account;

            return result.IdToken;
        }

        public async Task SignOut()
        {
            if (config.AppType == "Desktop")
            {
                if (authProvider.UserAccount != null)
                {
                    await authProvider.PubClient.RemoveAsync(authProvider.UserAccount);
                }
            }
            else if (config.AppType == "Web")
            {
                if (authProvider.UserAccount != null)
                {
                    await authProvider.ConfClient.RemoveAsync(authProvider.UserAccount);
                }
            }
            authProvider.UserAccount = null;
            IsInitialized = false;
            GraphServiceClient = null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
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
        
        private readonly ILogger<AuthorizationProvider> _logger;
        private readonly IOptionsMonitor<BaseConfig> _configMonitor;
        private readonly IDisposable? _reloadSubscription;

        private readonly object _sync = new();
        
        public IAccount UserAccount { get; set; }

        public AuthorizationProvider(IOptionsMonitor<BaseConfig> configMonitor, ILogger<AuthorizationProvider> logger)
        {
            _logger = logger;
            _configMonitor = configMonitor;
        }
    
        public bool RebuildMsalClients()
        {
            lock (_sync)
            {
                BaseConfig config = _configMonitor.CurrentValue;
                if (config.AppType == "Desktop")
                {
                    if (!Helpers.AreStringsNotEmpty(new string[] {
                        config.AADSettings.ClientId,
                        config.AADSettings.TenantId,
                        config.AADSettings.Instance,
                        config.AADSettings.RedirectUri }))
                    {
                        _logger.LogWarning("One or more of ClientId, TenantId, Instance, or RedirectUri is not set.");
                        PubClient = null;
                        return false;
                    }

                    PubClient = PublicClientApplicationBuilder.Create(config.AADSettings.ClientId)
                        .WithAuthority($"{config.AADSettings.Instance}{config.AADSettings.TenantId}/")
                        .WithRedirectUri(config.AADSettings.RedirectUri)
                        .Build();

                    TokenCacheHelper.EnableSerialization(PubClient.UserTokenCache);
                    return true;
                }
                else if (config.AppType == "Web")
                {
                    if (!Helpers.AreStringsNotEmpty(new string[] {
                        config.AADSettings.ClientId,
                        config.AADSettings.ClientSecret,
                        config.AADSettings.Instance,
                        config.AADSettings.RedirectHost,
                        config.AADSettings.CallbackPath }))
                    {
                        _logger.LogWarning("One or more of ClientId, ClientSecret, Instance, RedirectUri, or CallbackPath is not set.");
                        ConfClient = null;
                        return false;
                    }

                    ConfClient = ConfidentialClientApplicationBuilder
                        .Create(config.AADSettings.ClientId)
                        .WithClientSecret(config.AADSettings.ClientSecret)
                        .WithAuthority($"{config.AADSettings.Instance}{config.AADSettings.TenantId}/v2.0")
                        .WithRedirectUri($"{config.AADSettings.RedirectHost}{config.AADSettings.CallbackPath}")
                        .Build();

                    var cacheHelper = CreateCacheHelperAsync(config.AADSettings.ClientId);
                    cacheHelper.RegisterCache(ConfClient.UserTokenCache);
                    return true;
                }
            }

            return false;
        }

        

        public void Invalidate()
        {
            lock (_sync)
            {
                PubClient = null;
                ConfClient = null;
                UserAccount = null;
            }
        }


        public async Task<string> AcquireToken()
        {
            AuthenticationResult authResult = null;
            string accessToken = null;
            BaseConfig config = _configMonitor.CurrentValue;
            if (config.AppType == "Desktop")
            {
                var accounts = await PubClient.GetAccountsAsync();
                var firstAccount = accounts.FirstOrDefault();

                try
                {
                    _logger.LogTrace("Acquiring token silently");
                    authResult = await PubClient.AcquireTokenSilent(config.AADSettings.Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();

                    UserAccount = authResult.Account;
                    accessToken = authResult.AccessToken;
                    _logger.LogDebug("Got tokens silently");
                }
                catch (MsalUiRequiredException)
                {
                    _logger.LogInformation("Silent token acquisition failed. Falling back to interactive.");
                    try
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
                        //without a timeout, this hangs indefinitely if the browser/tab is closed
                        authResult = await PubClient.AcquireTokenInteractive(config.AADSettings.Scopes)
                           .WithUseEmbeddedWebView(false)
                           .ExecuteAsync(cts.Token);
                        _logger.LogTrace("Getting tokens interactively");
                        UserAccount = authResult.Account;
                        _logger.LogDebug("Got user account");
                        accessToken = authResult.AccessToken;
                        _logger.LogDebug("Got tokens interactively");
                    }
                    catch (MsalException ex) when (ex.ErrorCode == "access_denied")
                    {
                        // User closed the browser / cancelled login
                        _logger.LogWarning("User canceled interactive login");
                    }
                    catch (MsalException ex) when (ex.ErrorCode == "consent_required")
                    {
                        _logger.LogWarning("User did not consent to the application");
                    }
                    catch (MsalException ex) when (ex.ErrorCode == "invalid_request")
                    {
                        if (ex.Message != null && ex.Message.Contains("not configured as a multi-tenant application"))
                        {
                            _logger.LogWarning("Application is not configured as multi-tenant");
                        }
                        else
                        {
                            _logger.LogWarning(ex, "Interactive token acquisition failed");    
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning("Interactive login canceled or timed out");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Interactive token acquisition failed");
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

        public static bool AadChanged(BaseConfig config, BaseConfig newConfig)
        {
            if (config.AppType != newConfig.AppType)
            {
                return true;
            }
            else if (config.AppType == "Desktop")
            {
                bool aadChanged =
                       config.AADSettings.ClientId != newConfig.AADSettings.ClientId ||
                       config.AADSettings.TenantId != newConfig.AADSettings.TenantId ||
                       config.AADSettings.Instance != newConfig.AADSettings.Instance ||
                       config.AADSettings.RedirectUri != newConfig.AADSettings.RedirectUri;
                return aadChanged;
            }
            else if (config.AppType == "Web")
            {
                bool aadChanged =
                       config.AADSettings.ClientId != newConfig.AADSettings.ClientId ||
                       config.AADSettings.TenantId != newConfig.AADSettings.TenantId ||
                       config.AADSettings.Instance != newConfig.AADSettings.Instance ||
                       config.AADSettings.CallbackPath != newConfig.AADSettings.CallbackPath ||
                       config.AADSettings.RedirectHost != newConfig.AADSettings.RedirectHost ||
                       config.AADSettings.ClientSecret != newConfig.AADSettings.ClientSecret;
                return aadChanged;
            }

            return false;
        }
    }
}


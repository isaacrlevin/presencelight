using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Identity.Client;

namespace PresenceLight.Graph
{
    static class TokenCacheHelper
    {
        public static void EnableSerialization(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(BeforeAccessNotification);
            tokenCache.SetAfterAccess(AfterAccessNotification);
        }

        /// <summary>
        /// Path to the token cache. Note that this could be something different for instance for MSIX applications:
        /// </summary>
        private static readonly string CacheFolderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\PresenceLight\\";
        private static readonly string CacheFileName = "msalcache.bin";

        private static readonly object FileLock = new object();


        private static void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (FileLock)
            {
                args.TokenCache.DeserializeMsalV3(File.Exists($"{CacheFolderPath}{CacheFileName}")
                        ? ProtectedData.Unprotect(File.ReadAllBytes($"{CacheFolderPath}{CacheFileName}"),
                                                  null,
                                                  DataProtectionScope.CurrentUser)
                        : null);
            }
        }

        private static void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                lock (FileLock)
                {
                    // reflect changesgs in the persistent store

                    if (!Directory.Exists(CacheFolderPath))
                    {
                        Directory.CreateDirectory(CacheFolderPath);
                    }

                    File.WriteAllBytes($"{CacheFolderPath}{CacheFileName}",
                                        ProtectedData.Protect(args.TokenCache.SerializeMsalV3(),
                                                                null,
                                                                DataProtectionScope.CurrentUser)
                                        );
                }
            }
        }
    }
}

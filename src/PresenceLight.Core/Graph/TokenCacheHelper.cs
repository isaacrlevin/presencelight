using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Identity.Client;
using Windows.Storage;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;

namespace PresenceLight.Core.Graph
{
    static class TokenCacheHelper
    {
        /// <summary>
        /// Path to the token cache
        /// </summary>
        private static readonly string CacheFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location + ".msalcache.bin";

        private const string SETTINGS_FILENAME = "token.json";
        private static readonly StorageFolder _settingsFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        private static readonly object FileLock = new object();

        private static async void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            try
            {
                lock (FileLock)
                {
                    args.TokenCache.DeserializeMsalV3(File.Exists(CacheFilePath)
                        ? ProtectedData.Unprotect(File.ReadAllBytes(CacheFilePath),
                                                  null,
                                                  DataProtectionScope.CurrentUser)
                        : null);
                }
            }
            catch
            {
                args.TokenCache.DeserializeMsalV3(await IsFilePresent()
                     ? ProtectedData.Unprotect(await LoadFile(),
                                               null,
                                               DataProtectionScope.CurrentUser)
                     : null);

            }
        }

        private static async void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                try
                {
                    lock (FileLock)
                    {
                        // reflect changes in the persistent store
                        File.WriteAllBytes(CacheFilePath,
                                           ProtectedData.Protect(args.TokenCache.SerializeMsalV3(),
                                                                 null,
                                                                 DataProtectionScope.CurrentUser)
                                          );
                    }
                }
                catch
                {
                    // reflect changes in the persistent store
                    await SaveFile(ProtectedData.Protect(args.TokenCache.SerializeMsalV3(),
                                                                 null,
                                                                 DataProtectionScope.CurrentUser)
                                          );
                }
            }
        }

        private async static Task<byte[]> LoadFile()
        {
            try
            {
                StorageFile sf = await _settingsFolder.GetFileAsync(SETTINGS_FILENAME);
                if (sf == null) return null;

                var content = await FileIO.ReadBufferAsync(sf);
                return content.ToArray();
            }
            catch
            { return null; }
        }

        private async static Task<bool> SaveFile(byte[] content)
        {
            try
            {
                StorageFile file = await _settingsFolder.CreateFileAsync(SETTINGS_FILENAME, CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteBytesAsync(file, content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> IsFilePresent()
        {
            try
            {
                var item = await _settingsFolder.TryGetItemAsync(SETTINGS_FILENAME);
                return item != null;
            }
            catch
            {
                return false;
            }
        }
        internal static void EnableSerialization(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(BeforeAccessNotification);
            tokenCache.SetAfterAccess(AfterAccessNotification);
        }
    }
}

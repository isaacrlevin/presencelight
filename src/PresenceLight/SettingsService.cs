using Newtonsoft.Json;
using PresenceLight.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PresenceLight
{
    public static class SettingsService
    {
        private const string SETTINGS_FILENAME = "settings.json";
        private static StorageFolder _settingsFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        public async static Task<ConfigWrapper> LoadSettings()
        {
            try
            {
                StorageFile sf = await _settingsFolder.GetFileAsync(SETTINGS_FILENAME);
                if (sf == null) return null;

                string content = await FileIO.ReadTextAsync(sf);
                return JsonConvert.DeserializeObject<ConfigWrapper>(content);
            }
            catch
            { return null; }
        }

        public async static Task<bool> SaveSettings(ConfigWrapper data)
        {
            try
            {
                StorageFile file = await _settingsFolder.CreateFileAsync(SETTINGS_FILENAME, CreationCollisionOption.ReplaceExisting);
                string content = JsonConvert.SerializeObject(data);
                await FileIO.WriteTextAsync(file, content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> IsFilePresent(string fileName)
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
    }
}

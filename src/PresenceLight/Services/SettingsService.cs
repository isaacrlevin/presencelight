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
        private static readonly StorageFolder _settingsFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        public async static Task<ConfigWrapper> LoadSettings()
        {
            try
            {
                StorageFile sf = await _settingsFolder.GetFileAsync(SETTINGS_FILENAME);
                if (sf == null) return null;

                string content = await FileIO.ReadTextAsync(sf);
                return JsonConvert.DeserializeObject<ConfigWrapper>(content);
            }
            catch (Exception e)
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
            catch (Exception e)
            {
                return false;
            }
        }

        public async static Task<bool> DeleteSettings()
        {
            try
            {
                StorageFile sf = await _settingsFolder.GetFileAsync(SETTINGS_FILENAME);
                var foo = sf.DeleteAsync(StorageDeleteOption.PermanentDelete);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static async Task<bool> IsFilePresent()
        {
            try
            {
                var item = await _settingsFolder.TryGetItemAsync(SETTINGS_FILENAME);

                if (item == null)
                {
                    return false;
                }
                else
                {
                    var config = await LoadSettings();
                    if (config == null)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}

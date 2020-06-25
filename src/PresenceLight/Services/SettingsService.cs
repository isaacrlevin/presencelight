using System;
using Newtonsoft.Json;
using PresenceLight.Core;
using PresenceLight.Telemetry;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;
using Windows.Storage.Streams;

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

                string content = await FileIO.ReadTextAsync(sf, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                return JsonConvert.DeserializeObject<ConfigWrapper>(content);
            }
            catch (Exception e)
            {
                DiagnosticsClient.TrackException(e);
                return null;
            }
        }

        public async static Task<bool> SaveSettings(ConfigWrapper data)
        {
            try
            {
                string content = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { });
                StorageFile f;
                if (await IsFilePresent())
                {
                    f = await _settingsFolder.GetFileAsync(SETTINGS_FILENAME);
                }
                else
                {
                    f = await _settingsFolder.CreateFileAsync(SETTINGS_FILENAME, CreationCollisionOption.ReplaceExisting);
                }
                await FileIO.WriteTextAsync(f, content, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                //using (StorageStreamTransaction transaction = await f.OpenTransactedWriteAsync())
                //{

                //    using (DataWriter dataWriter = new DataWriter(transaction.Stream))
                //    {
                //        dataWriter.WriteString(content);

                //        transaction.Stream.Size = await dataWriter.StoreAsync();
                //        await transaction.CommitAsync();
                //    }
                //}
                return true;
            }
            catch (Exception e)
            {
                DiagnosticsClient.TrackException(e);
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
                DiagnosticsClient.TrackException(e);
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
                DiagnosticsClient.TrackException(e);
                return false;
            }
        }
    }
}

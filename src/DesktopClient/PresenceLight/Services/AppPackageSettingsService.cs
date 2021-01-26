using System;
using Newtonsoft.Json;
using PresenceLight.Core;
using PresenceLight.Telemetry;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Extensions.Logging;

namespace PresenceLight.Services
{
    public class AppPackageSettingsService : ISettingsService
    {
        private const string SETTINGS_FILENAME = "settings.json";
        private static readonly StorageFolder _settingsFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        private DiagnosticsClient _diagClient;
        private readonly ILogger<AppPackageSettingsService> _logger;

        public AppPackageSettingsService(DiagnosticsClient diagClient, ILogger<AppPackageSettingsService> logger)
        {
            _logger = logger;
            _diagClient = diagClient;
        }

        public async Task<BaseConfig?> LoadSettings()
        {
            try
            {
                StorageFile sf = await _settingsFolder.GetFileAsync(SETTINGS_FILENAME);
                if (sf == null) return null;

                string content = await FileIO.ReadTextAsync(sf, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                return JsonConvert.DeserializeObject<BaseConfig>(content);
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error saving Settings", e);
                _diagClient.TrackException(e);
                return null;
            }
        }

        public async Task<bool> SaveSettings(BaseConfig data)
        {
            try
            {
                string content = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { });
                StorageFile f;
                if (await IsFilePresent().ConfigureAwait(true))
                {
                    f = await _settingsFolder.GetFileAsync(SETTINGS_FILENAME);
                }
                else
                {
                    f = await _settingsFolder.CreateFileAsync(SETTINGS_FILENAME, CreationCollisionOption.ReplaceExisting);
                }
                bool fileWritten = false;

                while (!fileWritten)
                {
                    try
                    {
                        await FileIO.WriteTextAsync(f, content, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                        fileWritten = true;
                    }
                    catch
                    {                     
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error Saving Settings", e);
                _diagClient.TrackException(e);
                return false;
            }
        }

        public async Task<bool> DeleteSettings()
        {
            try
            {
                StorageFile sf = await _settingsFolder.GetFileAsync(SETTINGS_FILENAME);
                var foo = sf.DeleteAsync(StorageDeleteOption.PermanentDelete);
                return true;
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error Deleting Settings File", e);
                _diagClient.TrackException(e);
                return false;
            }
        }

        public async Task<bool> IsFilePresent()
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
                    var config = await LoadSettings().ConfigureAwait(true);
                    if (config == null)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error Finding Settings File", e);
                _diagClient.TrackException(e);
                return false;
            }
        }
    }
}

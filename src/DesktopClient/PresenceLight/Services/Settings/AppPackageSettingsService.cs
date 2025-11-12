using System;
using Newtonsoft.Json;
using PresenceLight.Core;
using PresenceLight.Telemetry;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Extensions.Logging;
using System.IO;
using PresenceLight.Razor;

namespace PresenceLight.Services
{
    public class AppPackageSettingsService : ISettingsService
    {
        private const string SETTINGS_FILENAME = "settings.json";
        private static readonly StorageFolder _settingsFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        private DiagnosticsClient _diagClient;
        private readonly ILogger<AppPackageSettingsService> _logger;
        private readonly AppState _appState;

        public AppPackageSettingsService(DiagnosticsClient diagClient, ILogger<AppPackageSettingsService> logger, AppState appState)
        {
            _appState = appState;
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
                var config = JsonConvert.DeserializeObject<BaseConfig>(content);
                _appState.SetConfig(config);
                return config;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error saving Settings");
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
                if (await IsFilePresent())
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
                _appState.SetConfig(data);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Saving Settings");
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
                _logger.LogError(e, "Error Deleting Settings File");
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
                _logger.LogError(e, "Error Finding Settings File");
                _diagClient.TrackException(e);
                return false;
            }
        }

        public string GetSettingsFileLocation()
        {
            return BuildSettingsFileLocation();
        }

        public static string BuildSettingsFileLocation()
        {
            return Path.Combine(_settingsFolder.Path, SETTINGS_FILENAME);
        }
    }
}

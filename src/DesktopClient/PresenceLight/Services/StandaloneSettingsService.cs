using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PresenceLight.Core;
using PresenceLight.Telemetry;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace PresenceLight.Services
{
    public class StandaloneSettingsService : ISettingsService
    {
        private const string _settingsFileName = "settings.json";
        private static readonly string _settingsFolder = Directory.GetCurrentDirectory();
        private DiagnosticsClient _diagClient;
        private readonly ILogger<StandaloneSettingsService> _logger;

        public StandaloneSettingsService(DiagnosticsClient diagClient, ILogger<StandaloneSettingsService> logger)
        {
            _logger = logger;
            _diagClient = diagClient;
        }

        public Task<bool> DeleteSettings()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsFilePresent()
        {
            try
            {
                if (!File.Exists(Path.Combine(_settingsFolder, _settingsFileName)))
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
                _logger.LogError(e, "Error Finding Settings File");
                _diagClient.TrackException(e);
                return false;
            }
        }

        public async Task<BaseConfig?> LoadSettings()
        {
            try
            {
                string fileJSON = await File.ReadAllTextAsync(Path.Combine(_settingsFolder, _settingsFileName), Encoding.UTF8).ConfigureAwait(true);
                return JsonConvert.DeserializeObject<BaseConfig>(fileJSON);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Loading Settings");
                _diagClient.TrackException(e);
                return null;
            }
        }

        public async Task<bool> SaveSettings(BaseConfig data)
        {
            try
            {
                string content = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { });
                await File.WriteAllTextAsync(Path.Combine(_settingsFolder, _settingsFileName), content, Encoding.UTF8).ConfigureAwait(false);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error saving Settings");
                _diagClient.TrackException(e);
                return false;
            }
        }
    }
}

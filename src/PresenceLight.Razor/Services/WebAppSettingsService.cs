using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

using PresenceLight.Core;

namespace PresenceLight.Razor.Services
{
    public class WebAppSettingsService : ISettingsService
    {
        readonly IConfiguration _configuration;
        private readonly AppState _appState;
        public WebAppSettingsService(IConfiguration configuration, AppState appState)
        {
            _appState = appState;
            _configuration = configuration;
        }

        public Task<bool> DeleteSettings()
        {
            if (File.Exists(GetSettingsFileLocation()))
            {
                File.Delete(GetSettingsFileLocation());
            }
            return Task.Run(() => true);
        }

        public async Task<bool> IsFilePresent()
        {
            if (!File.Exists(GetSettingsFileLocation()))
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

        public async Task<BaseConfig?> LoadSettings()
        {
            string fileJSON = await File.ReadAllTextAsync(GetSettingsFileLocation(), Encoding.UTF8);
            var config = JsonConvert.DeserializeObject<BaseConfig>(fileJSON);
            _appState.SetConfig(config);
            return config;
        }

        public async Task<bool> SaveSettings(BaseConfig data)
        {
            string content = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { });
            await File.WriteAllTextAsync(GetSettingsFileLocation(), content, Encoding.UTF8);
            _appState.SetConfig(data);
            return true;
        }

        public string GetSettingsFileLocation()
        {

            if (_configuration?["DOTNET_RUNNING_IN_CONTAINER"] == "true")
            {
                return System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "config", "PresenceLightSettings.json");
            }
            else if (Debugger.IsAttached)
            {
                return System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "PresenceLightSettings.Development.json");
            }
            else
            {
                return System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "PresenceLightSettings.json");
            }
        }
    }
}

using System.Diagnostics;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

using PresenceLight.Core;

namespace PresenceLight.Worker.Services
{
    public class SettingsService
    {
        readonly IConfiguration _configuration;
        public SettingsService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void SaveSettings(BaseConfig Config)
        {

            System.IO.File.WriteAllText(SettingsFileLocation, JsonConvert.SerializeObject(Config, Formatting.Indented));
        }

        public string SettingsFileLocation
        {
            get
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
                    return System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "PresenceLightSettings.json");

            }
        }
    }
}

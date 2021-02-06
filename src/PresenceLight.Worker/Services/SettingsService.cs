using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

using PresenceLight.Core;

namespace PresenceLight.Worker.Services
{
    public static class SettingsService
    {
        public static void SaveSettings(BaseConfig Config, IConfiguration configuration = null)
        {
            if (configuration?["DOTNET_RUNNING_IN_CONTAINER"] == "true")
            {
                var filePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "config", "PresenceLightSettings.json");
                System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(Config, Formatting.Indented));
            }
            else if (Debugger.IsAttached)
            {
                System.IO.File.WriteAllText($"{System.IO.Directory.GetCurrentDirectory()}/PresenceLightSettings.Development.json", JsonConvert.SerializeObject(Config, Formatting.Indented));
            }
            else
            {
                System.IO.File.WriteAllText($"{System.IO.Directory.GetCurrentDirectory()}/PresenceLightSettings.json", JsonConvert.SerializeObject(Config, Formatting.Indented));
            }
        }
    }
}

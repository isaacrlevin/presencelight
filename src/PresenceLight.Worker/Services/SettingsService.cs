using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

using PresenceLight.Core;

namespace PresenceLight.Worker.Services
{
    public static class SettingsService
    {
        public static void SaveSettings(BaseConfig Config)
        {
            if (Debugger.IsAttached)
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

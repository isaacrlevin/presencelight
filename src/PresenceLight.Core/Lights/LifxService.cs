using LifxCloud.NET;
using LifxCloud.NET.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PresenceLight.Core
{
   public class LifxService
    {
        private readonly ConfigWrapper _options;
        private LifxCloudClient client;

        public LifxService(Microsoft.Extensions.Options.IOptionsMonitor<ConfigWrapper> optionsAccessor)
        {
            _options = optionsAccessor.CurrentValue;
        }

        public async Task<List<Light>> GetLightsAsync()
        {
            client = await LifxCloudClient.CreateAsync(_options.LifxApiKey);
            return await client.ListLights();
        }
        public async Task SetColor(string availability)
        {
            client = await LifxCloudClient.CreateAsync(_options.LifxApiKey);
            string color = "";
            switch (availability)
            {
                case "Available":
                    color = "green";
                    break;
                case "Busy":
                    color = "red";
                    break;
                case "BeRightBack":
                    color = "yellow";
                    break;
                case "Away":
                    color = "yellow";
                    break;
                case "DoNotDisturb":
                    color = "purple";
                    break;
                default:
                    color = "white";
                    break;
            }

            await client.SetAllState(new LifxCloud.NET.Models.SetStateRequest { 
             color = color
            });
        }
    }
}

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

        public async Task<List<Light>> GetAllLightsAsync()
        {
            client = await LifxCloudClient.CreateAsync(_options.LifxApiKey);
            return await client.ListLights(Selector.All);
        }

        public async Task<List<Group>> GetAllGroupsAsync()
        {
            client = await LifxCloudClient.CreateAsync(_options.LifxApiKey);
            return await client.ListGroups(Selector.All);
        }
        public async Task SetColor(string availability, Selector selector)
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
                case "Off":
                    color = "white";
                    break;
                default:
                    color = availability;
                    break;
            }

            var result = await client.SetState(selector,new LifxCloud.NET.Models.SetStateRequest
            {
                Color = color
            });
        }
    }
}

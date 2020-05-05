using System;
using LifxCloud.NET;
using LifxCloud.NET.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PresenceLight.Core
{
    public class LIFXService
    {
        private readonly ConfigWrapper _options;
        private LifxCloudClient _client;

        public LIFXService(Microsoft.Extensions.Options.IOptionsMonitor<ConfigWrapper> optionsAccessor)
        {
            _options = optionsAccessor.CurrentValue;
        }

        public async Task<List<Light>> GetAllLightsAsync()
        {
            if (!_options.IsLIFXEnabled || string.IsNullOrEmpty(_options.LIFXApiKey))
            {
                return new List<Light>();
            }

            _client = await LifxCloudClient.CreateAsync(_options.LIFXApiKey);
            return await _client.ListLights(Selector.All);
        }

        public async Task<List<Group>> GetAllGroupsAsync()
        {
            if (!_options.IsLIFXEnabled || string.IsNullOrEmpty(_options.LIFXApiKey))
            {
                return new List<Group>();
            }
            _client = await LifxCloudClient.CreateAsync(_options.LIFXApiKey);
            return await _client.ListGroups(Selector.All);
        }
        public async Task SetColor(string availability, Selector selector)
        {
            if (!_options.IsLIFXEnabled || string.IsNullOrEmpty(_options.LIFXApiKey))
            {
                return;
            }
            _client = await LifxCloudClient.CreateAsync(_options.LIFXApiKey);
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
                case "Offline":
                    color = "white";
                    break;
                case "Off":
                    color = "white";
                    break;
                default:
                    color = availability;
                    break;
            }
            if (_options.Brightness == 0)
            {
                var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                {
                    Power = PowerState.Off
                }); ;
            }
            else
            {
                var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                {
                    Brightness = (_options.Brightness / 100),
                    Color = color
                }); ;
            }
        }
    }
}

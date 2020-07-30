using System;
using LifxCloud.NET;
using LifxCloud.NET.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Capture;

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
            if (!_options.LightSettings.LIFX.IsLIFXEnabled || string.IsNullOrEmpty(_options.LightSettings.LIFX.LIFXApiKey))
            {
                return new List<Light>();
            }

            _client = await LifxCloudClient.CreateAsync(_options.LightSettings.LIFX.LIFXApiKey);
            return await _client.ListLights(Selector.All);
        }

        public async Task<List<Group>> GetAllGroupsAsync()
        {
            if (!_options.LightSettings.LIFX.IsLIFXEnabled || string.IsNullOrEmpty(_options.LightSettings.LIFX.LIFXApiKey))
            {
                return new List<Group>();
            }
            _client = await LifxCloudClient.CreateAsync(_options.LightSettings.LIFX.LIFXApiKey);
            return await _client.ListGroups(Selector.All);
        }
        public async Task SetColor(string availability, Selector selector)
        {
            if (!_options.LightSettings.LIFX.IsLIFXEnabled || string.IsNullOrEmpty(_options.LightSettings.LIFX.LIFXApiKey))
            {
                return;
            }
            _client = await LifxCloudClient.CreateAsync(_options.LightSettings.LIFX.LIFXApiKey);
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

            if (availability == "Off")
            {
                var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                {
                    Color = color,
                    Power = PowerState.Off
                });
                return;
            }


            if (_options.LightSettings.UseDefaultBrightness)
            {
                if (_options.LightSettings.DefaultBrightness == 0)
                {
                    var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                    {
                        Power = PowerState.Off
                    });
                }
                else
                {
                    var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                    {
                        Brightness = Convert.ToDouble(_options.LightSettings.DefaultBrightness) / 100,
                        Color = color,
                        Duration = 0
                    });
                }
            }
            else
            {
                if (_options.LightSettings.Hue.HueBrightness == 0)
                {
                    var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                    {
                        Power = PowerState.Off
                    });
                }
                else
                {
                    var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                    {
                        Brightness = Convert.ToDouble(_options.LightSettings.LIFX.LIFXBrightness) / 100,
                        Color = color,
                        Duration = 0
                    });
                }
            }
        }
    }
}

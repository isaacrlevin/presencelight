using Microsoft.Extensions.Options;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Original;
using Q42.HueApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PresenceLight.Core
{
    public interface IHueService
    {
        Task SetColor(string availability);
    }
    public class HueService : IHueService
    {
        private readonly ILocalHueClient _client;
        private readonly ConfigWrapper _options;

        public HueService(IOptionsMonitor<ConfigWrapper> optionsAccessor)
        {
            _options = optionsAccessor.CurrentValue;
            ILocalHueClient _client = new LocalHueClient(_options.HueIpAddress);
            _client.Initialize(_options.HueApiKey);
        }

        public async Task SetColor(string availability)
        {
            var command = new LightCommand
            {
                On = true
            };
            switch (availability)
            {
                case "Available":
                    command.SetColor(new RGBColor("#00FF00"));
                    break;
                case "Busy":
                    command.SetColor(new RGBColor("#FF0000"));
                    break;
                case "BeRightBack":
                    command.SetColor(new RGBColor("#ffff00"));
                    break;
                case "Away":
                    command.SetColor(new RGBColor("#ffff00"));
                    break;
                case "DoNotDisturb":
                    command.SetColor(new RGBColor("#B31B1B"));
                    break;
                default:
                    command.SetColor(new RGBColor("#FFFFFF"));
                    break;
            }
            await _client.SendCommandAsync(command);
        }
    }
}

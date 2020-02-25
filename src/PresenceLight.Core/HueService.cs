using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.HSB;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models.Bridge;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core
{
    public interface IHueService
    {
        Task SetColor(string availability);
        Task RegisterBridge();
    }
    public class HueService : IHueService
    {
        private readonly ConfigWrapper _options;
        private LocalHueClient _client;

        public HueService(IOptionsMonitor<ConfigWrapper> optionsAccessor)
        {
            _options = optionsAccessor.CurrentValue;
        }

        public HueService(ConfigWrapper options)
        {
            _options = options;
        }

        public async Task SetColor(string availability)
        {           
            _client = new LocalHueClient(_options.HueIpAddress);
            _client.Initialize(_options.HueApiKey);

            CheckLights();

            var command = new LightCommand
            {
                On = true
            };
            switch (availability)
            {
                case "Available":
                    command.SetColor(new RGBColor("#009933"));
                    break;
                case "Busy":
                    command.SetColor(new RGBColor("#ff3300"));
                    break;
                case "BeRightBack":
                    command.SetColor(new RGBColor("#ffff00"));
                    break;
                case "Away":
                    command.SetColor(new RGBColor("#ffff00"));
                    break;
                case "DoNotDisturb":
                    command.SetColor(new RGBColor("#800000"));
                    break;
                default:
                    command.SetColor(new RGBColor("#FFFFFF"));
                    break;
            }
            await _client.SendCommandAsync(command);
        }

        //Need to wire up a way to do this without user intervention
        public async Task RegisterBridge()
        {
            if (string.IsNullOrEmpty(_options.HueApiKey))
            {
                //IBridgeLocator locator = new HttpBridgeLocator(); //Or: LocalNetworkScanBridgeLocator, MdnsBridgeLocator, MUdpBasedBridgeLocator
                //var bridges = (List<LocatedBridge>)locator.LocateBridgesAsync(TimeSpan.FromSeconds(5)).Result;

                //_options.HueIpAddress = bridges[0].IpAddress;

                _client = new LocalHueClient(_options.HueIpAddress);
                //Make sure the user has pressed the button on the bridge before calling RegisterAsync
                //It will throw an LinkButtonNotPressedException if the user did not press the button
                _options.HueApiKey = await _client.RegisterAsync("presence-light", "presence-light");

                System.IO.File.WriteAllText($"{System.IO.Directory.GetCurrentDirectory()}/appsettings.json", JsonConvert.SerializeObject(_options));
            }
        }

        private void CheckLights()
        {
            List<Light> lights = (List<Light>)_client.GetLightsAsync().Result;

            // if there are no lights, get some
            if (lights.Count == 0)
            {
                _client.SearchNewLightsAsync();
                Thread.Sleep(40000);
                _client.GetNewLightsAsync();
            }
        }
    }
}

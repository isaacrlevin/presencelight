using System;
using Microsoft.Extensions.Options;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.HSB;
using Q42.HueApi.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PresenceLight.Core
{
    public interface IHueService
    {
        Task SetColor(string availability, string lightId);
        Task<string> RegisterBridge();
        Task<IEnumerable<Light>> GetLights();
        Task<string> FindBridge();
    }
    public class HueService : IHueService
    {
        private readonly BaseConfig _options;
        private LocalHueClient _client;
        private readonly ILogger<HueService> _logger;

        public HueService(IOptionsMonitor<BaseConfig> optionsAccessor, ILogger<HueService> logger)
        {
            _logger = logger;
            _options = optionsAccessor.CurrentValue;
        }

        public HueService(BaseConfig options)
        {
            _options = options;
        }

        public async Task SetColor(string availability, string lightId)
        {
            if (string.IsNullOrEmpty(lightId))
            {
                throw new ArgumentOutOfRangeException("Hue Selected Light Id Invalid");
            }

            try
            {
                _client = new LocalHueClient(_options.LightSettings.Hue.HueIpAddress);
                _client.Initialize(_options.LightSettings.Hue.HueApiKey);

                string message = "";

                var command = new LightCommand();
                string color = "";
                switch (availability)
                {
                    case "Available":
                        if (!_options.LightSettings.Hue.AvailableStatus.Disabled)
                        {
                            command.On = true;
                            color = _options.LightSettings.Hue.AvailableStatus.Colour;
                        }
                        else
                        {
                            command.On = false;
                            await _client.SendCommandAsync(command, new List<string> { lightId });
                            message = $"Turning Hue Light {lightId} Off";
                            Helpers.AppendLogger(_logger, message);
                            return;
                        }
                        break;
                    case "Busy":
                        if (!_options.LightSettings.Hue.BusyStatus.Disabled)
                        {
                            command.On = true;
                            color = _options.LightSettings.Hue.BusyStatus.Colour;
                        }
                        else
                        {
                            command.On = false;
                            await _client.SendCommandAsync(command, new List<string> { lightId });
                            message = $"Turning Hue Light {lightId} Off";
                            Helpers.AppendLogger(_logger, message);
                            return;
                        }
                        break;
                    case "BeRightBack":
                        if (!_options.LightSettings.Hue.BeRightBackStatus.Disabled)
                        {
                            command.On = true;
                            color = _options.LightSettings.Hue.BeRightBackStatus.Colour;
                        }
                        else
                        {
                            command.On = false;
                            await _client.SendCommandAsync(command, new List<string> { lightId });
                            message = $"Turning Hue Light {lightId} Off";
                            Helpers.AppendLogger(_logger, message);
                            return;
                        }
                        break;
                    case "Away":
                        if (!_options.LightSettings.Hue.AwayStatus.Disabled)
                        {
                            command.On = true;
                            color = _options.LightSettings.Hue.AwayStatus.Colour;
                        }
                        else
                        {
                            command.On = false;
                            await _client.SendCommandAsync(command, new List<string> { lightId });
                            message = $"Turning Hue Light {lightId} Off";
                            Helpers.AppendLogger(_logger, message);
                            return;
                        }
                        break;
                    case "DoNotDisturb":
                        if (!_options.LightSettings.Hue.DoNotDisturbStatus.Disabled)
                        {
                            command.On = true;
                            color = _options.LightSettings.Hue.DoNotDisturbStatus.Colour;
                        }
                        else
                        {
                            command.On = false;
                            await _client.SendCommandAsync(command, new List<string> { lightId });
                            message = $"Turning Hue Light {lightId} Off";
                            Helpers.AppendLogger(_logger, message);
                            return;
                        }
                        break;
                    case "Offline":
                        if (!_options.LightSettings.Hue.OfflineStatus.Disabled)
                        {
                            command.On = true;
                            color = _options.LightSettings.Hue.OfflineStatus.Colour;
                        }
                        else
                        {
                            command.On = false;
                            await _client.SendCommandAsync(command, new List<string> { lightId });
                            message = $"Turning Hue Light {lightId} Off";
                            Helpers.AppendLogger(_logger, message);
                            return;
                        }
                        break;
                    case "Off":
                        if (!_options.LightSettings.Hue.OffStatus.Disabled)
                        {
                            command.On = true;
                            color = _options.LightSettings.Hue.OffStatus.Colour;
                        }
                        else
                        {
                            command.On = false;
                            await _client.SendCommandAsync(command, new List<string> { lightId });
                            message = $"Turning Hue Light {lightId} Off";
                            Helpers.AppendLogger(_logger, message);
                            return;
                        }
                        break;
                    default:
                        command.On = true;
                        color = availability;
                        break;
                }

                color = color.Replace("#", "");

                switch (color.Length)
                {

                    case var length when color.Length == 6:
                        // Do Nothing
                        break;
                    case var length when color.Length > 6:
                        // Get last 6 characters
                        color = color.Substring(color.Length - 6);
                        break;
                    default:
                        throw new ArgumentException("Supplied Color had an issue");
                }

                command.SetColor(new RGBColor(color));


                if (availability == "Off")
                {
                    command.On = false;
                    await _client.SendCommandAsync(command, new List<string> { lightId });
                    message = $"Turning Hue Light {lightId} Off";
                    Helpers.AppendLogger(_logger, message);
                    return;
                }

                if (_options.LightSettings.UseDefaultBrightness)
                {
                    if (_options.LightSettings.DefaultBrightness == 0)
                    {
                        command.On = false;
                    }
                    else
                    {
                        command.On = true;
                        command.Brightness = Convert.ToByte(((Convert.ToDouble(_options.LightSettings.DefaultBrightness) / 100) * 254));
                        command.TransitionTime = new TimeSpan(0);
                    }
                }
                else
                {
                    if (_options.LightSettings.Hue.HueBrightness == 0)
                    {
                        command.On = false;
                    }
                    else
                    {
                        command.On = true;
                        command.Brightness = Convert.ToByte(((Convert.ToDouble(_options.LightSettings.Hue.HueBrightness) / 100) * 254));
                        command.TransitionTime = new TimeSpan(0);
                    }
                }

                await _client.SendCommandAsync(command, new List<string> { lightId });
                message = $"Setting Hue Light {lightId} to {color}";
                Helpers.AppendLogger(_logger, message);
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error Occurred Setting Color", e);
                throw;
            }
        }

        //Need to wire up a way to do this without user intervention
        public async Task<string> RegisterBridge()
        {
            if (string.IsNullOrEmpty(_options.LightSettings.Hue.HueApiKey))
            {
                try
                {
                    _client = new LocalHueClient(_options.LightSettings.Hue.HueIpAddress);

                    //Make sure the user has pressed the button on the bridge before calling RegisterAsync
                    //It will throw an LinkButtonNotPressedException if the user did not press the button

                    return await _client.RegisterAsync("presence-light", "presence-light");
                }
                catch (Exception e)
                {
                    Helpers.AppendLogger(_logger, "Error Occurred Registering Bridge", e);
                    return String.Empty;
                }
            }
            return _options.LightSettings.Hue.HueApiKey;
        }

        public async Task<string> FindBridge()
        {
            try
            {
                IBridgeLocator locator = new HttpBridgeLocator(); //Or: LocalNetworkScanBridgeLocator, MdnsBridgeLocator, MUdpBasedBridgeLocator
                var bridges = await locator.LocateBridgesAsync(TimeSpan.FromSeconds(5));
                if (bridges.Count() > 0)
                {
                    return bridges.FirstOrDefault().IpAddress;
                }
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error Occurred Finding Bridge", e);
                return String.Empty;
            }
            return String.Empty;
        }

        public async Task<IEnumerable<Light>> GetLights()
        {
            try
            {
                if (_client == null)
                {
                    _client = new LocalHueClient(_options.LightSettings.Hue.HueIpAddress);
                    _client.Initialize(_options.LightSettings.Hue.HueApiKey);
                }
                var lights = await _client.GetLightsAsync();
                // if there are no lights, get some
                if (lights.Count() == 0)
                {
                    await _client.SearchNewLightsAsync();
                    Thread.Sleep(40000);
                    lights = await _client.GetNewLightsAsync();
                }
                return lights;
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error Occurred Getting Bridge", e);
                throw;
            }

        }
    }
}

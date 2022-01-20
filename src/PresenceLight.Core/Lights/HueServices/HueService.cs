using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.HSB;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models.Groups;

namespace PresenceLight.Core
{
    public interface IHueService
    {
        Task SetColor(string availability, string activity, string lightId);
        Task<string> RegisterBridge();
        Task<IEnumerable<Light>> GetLights();

        Task<IEnumerable<Group>> GetGroups();
        Task<string> FindBridge();
        void Initialize(BaseConfig options);
    }
    public class HueService : IHueService
    {
        private BaseConfig _options;
        private LocalHueClient _client;
        private readonly ILogger<HueService> _logger;
        MediatR.IMediator _mediator;
        public HueService(IOptionsMonitor<BaseConfig> optionsAccessor, MediatR.IMediator mediator, ILogger<HueService> logger)
        {
            _mediator = mediator;
            _logger = logger;
            _options = optionsAccessor.CurrentValue;
        }

        public void Initialize(BaseConfig options)
        {
            _options = options;
        }

        public async Task SetColor(string availability, string activity, string lightId)
        {
            if (string.IsNullOrEmpty(lightId))
            {
                _logger.LogInformation("Selected Hue Light Not Specified");
                return;
            }

            try
            {
                _client = new LocalHueClient(_options.LightSettings.Hue.HueIpAddress);
                _client.Initialize(_options.LightSettings.Hue.HueApiKey);

                var o = await Handle(_options.LightSettings.Hue.UseActivityStatus ? activity : availability, lightId);

                if (o.returnFunc)
                {
                    return;
                }

                var color = o.color.Replace("#", "");
                var command = o.command;
                var message = "";
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

                    if (lightId.Contains("group_id:"))
                    {
                        await _client.SendGroupCommandAsync(command, lightId.Replace("group_id:", ""));
                    }
                    else
                    {
                        await _client.SendCommandAsync(command, new List<string> { lightId.Replace("id:", "") });
                    }

                    message = $"Turning Hue Light {lightId} Off";
                    _logger.LogInformation(message);
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
                    if (_options.LightSettings.Hue.Brightness == 0)
                    {
                        command.On = false;
                    }
                    else
                    {
                        command.On = true;
                        command.Brightness = Convert.ToByte(((Convert.ToDouble(_options.LightSettings.Hue.Brightness) / 100) * 254));
                        command.TransitionTime = new TimeSpan(0);
                    }
                }

                if (lightId.Contains("group_id:"))
                {
                    await _client.SendGroupCommandAsync(command, lightId.Replace("group_id:", ""));
                }
                else
                {
                    await _client.SendCommandAsync(command, new List<string> { lightId.Replace("id:", "") });
                }

                message = $"Setting Hue Light {lightId} to {color}";
                _logger.LogInformation(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Occurred Setting Color");
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
                    _logger.LogError(e, "Error Occurred Registering Bridge");
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
                if (bridges.Any())
                {
                    return bridges.FirstOrDefault().IpAddress;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Occurred Finding Bridge");
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
                if (!lights.Any())
                {
                    await _client.SearchNewLightsAsync();
                    Thread.Sleep(40000);
                    lights = await _client.GetNewLightsAsync();
                }
                return lights;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Occurred Getting Bridge", e);
                throw;
            }
        }

        public async Task<IEnumerable<Group>> GetGroups()
        {
            try
            {
                if (_client == null)
                {
                    _client = new LocalHueClient(_options.LightSettings.Hue.HueIpAddress);
                    _client.Initialize(_options.LightSettings.Hue.HueApiKey);
                }
                return await _client.GetGroupsAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Occurred Getting Bridge", e);
                throw;
            }
        }

        private async Task<(string color, LightCommand command, bool returnFunc)> Handle(string presence, string lightId)
        {
            var props = _options.LightSettings.Hue.Statuses.GetType().GetProperties().ToList();

            if (_options.LightSettings.Hue.UseActivityStatus)
            {
                props = props.Where(a => a.Name.ToLower().StartsWith("activity")).ToList();
            }
            else
            {
                props = props.Where(a => a.Name.ToLower().StartsWith("availability")).ToList();
            }

            string color = "";
            string message;
            var command = new LightCommand();

            if (presence.Contains("#"))
            {
                // provided presence is actually a custom color
                color = presence;
                command.On = true;
                return (color, command, false);
            }

            foreach (var prop in props)
            {
                if (presence == prop.Name.Replace("Status", "").Replace("Availability", "").Replace("Activity", ""))
                {
                    var value = (AvailabilityStatus)prop.GetValue(_options.LightSettings.Hue.Statuses);

                    if (!value.Disabled)
                    {
                        command.On = true;
                        color = value.Colour;
                        return (color, command, false);
                    }
                    else
                    {
                        command.On = false;

                        if (lightId.Contains("group_id:"))
                        {
                            await _client.SendGroupCommandAsync(command, lightId.Replace("group_id:", ""));
                        }
                        else
                        {
                            await _client.SendCommandAsync(command, new List<string> { lightId.Replace("id:", "") });
                        }
                        message = $"Turning Hue Light {lightId} Off";
                        _logger.LogInformation(message);
                        return (color, command, true);
                    }
                }
            }
            return (color, command, false);
        }
    }
}

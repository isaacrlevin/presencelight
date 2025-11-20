using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HueApi;
using HueApi.BridgeLocator;
using HueApi.ColorConverters.Original.Extensions;
using HueApi.Models;
using HueApi.Models.Requests;

using Microsoft.Extensions.Logging;


namespace PresenceLight.Core
{
    public interface IHueService
    {
        Task SetColor(string availability, string activity, string lightId);
        Task<string> RegisterBridge();
        Task<IEnumerable<Light>> GetLights();

        Task<IEnumerable<GroupedLight>> GetGroups();
        Task<string> FindBridge();
        void Initialize(AppState appState);
    }
    public class HueService : IHueService
    {
        private AppState _appState;
        private LocalHueApi _client;
        private readonly ILogger<HueService> _logger;

        public HueService(AppState appState, ILogger<HueService> logger)
        {
            _logger = logger;
            _appState = appState;
        }

        public void Initialize(AppState appState)
        {
            _appState = appState;
        }

        public async Task SetColor(string availability, string activity, string lightId)
        {
            if (_appState.HueLights == null || _appState.HueLights.Count() == 0)
            {
                if (lightId.Contains("group_id:"))
                {
                    _appState.SetHueLights(await GetGroups());
                }
                else
                {
                    _appState.SetHueLights(await GetLights());
                }
            }

            if (string.IsNullOrEmpty(lightId))
            {
                _logger.LogInformation("Selected Hue Light Not Specified");
                return;
            }

            try
            {
                _client = new LocalHueApi(_appState.Config.LightSettings.Hue.HueIpAddress, _appState.Config.LightSettings.Hue.HueApiKey);

                var o = await Handle(_appState.Config.LightSettings.Hue.UseActivityStatus ? activity : availability, lightId);

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
                        color = color.Substring(0, 6);
                        break;
                    default:
                        throw new ArgumentException("Supplied Color had an issue");
                }

                var rgbColor = new HueApi.ColorConverters.RGBColor(color);
                // Set the color using extension method
                command.SetColor(rgbColor);



                if (availability == "Off")
                {
                    command.TurnOff();

                    if (lightId.Contains("group_id:"))
                    {
                        var groupCommand = new UpdateGroupedLight();
                        groupCommand.TurnOff();
                        await _client.UpdateGroupedLightAsync(Guid.Parse(lightId.Replace("group_id:", "")), groupCommand);
                    }
                    else
                    {
                        await _client.UpdateLightAsync(Guid.Parse(lightId.Replace("id:", "")), command);
                    }

                    message = $"Turning Hue Light {lightId} Off";
                    _logger.LogInformation(message);
                    return;
                }

                if (_appState.Config.LightSettings.UseDefaultBrightness)
                {
                    if (_appState.Config.LightSettings.DefaultBrightness == 0)
                    {
                        command.TurnOff();
                    }
                    else
                    {
                        command.TurnOn();
                        command.Dimming = new Dimming { Brightness = Convert.ToDouble(_appState.Config.LightSettings.DefaultBrightness) };
                        command.Dynamics = new Dynamics { Duration = 0 };
                    }
                }
                else
                {
                    if (_appState.Config.LightSettings.Hue.Brightness == 0)
                    {
                        command.TurnOff();
                    }
                    else
                    {
                        command.TurnOn();
                        command.Dimming = new Dimming { Brightness = Convert.ToDouble(_appState.Config.LightSettings.Hue.Brightness) };
                        command.Dynamics = new Dynamics { Duration = 0 };
                    }
                }

                if (lightId.Contains("group_id:"))
                {
                    var newLightId = ((GroupedLight)_appState.HueLights.First(a => ((GroupedLight)a).IdV1 == lightId.Replace("group_id:", ""))).Id;


                    var groupCommand = new UpdateGroupedLight();
                    groupCommand.Color = command.Color;
                    groupCommand.On = command.On;
                    groupCommand.Dimming = command.Dimming;
                    groupCommand.Dynamics = command.Dynamics;
                    await _client.GroupedLight.UpdateAsync(newLightId, groupCommand);
                }
                else
                {
                    var newLightId = ((Light)_appState.HueLights.First(a => ((Light)a).IdV1 == lightId.Replace("id:", ""))).Id;
                    await _client.Light.UpdateAsync(newLightId, command);
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
            if (string.IsNullOrEmpty(_appState.Config.LightSettings.Hue.HueApiKey))
            {
                try
                {
                    _logger.LogInformation("Registering with Hue Bridge - Please press the button on your bridge");
                    var result = await LocalHueApi.RegisterAsync(_appState.Config.LightSettings.Hue.HueIpAddress, "PresenceLight", Environment.MachineName, true);
                    return result.Username; // RegisterAsync returns RegisterEntertainmentResult with Username property
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error Occurred Registering Bridge");
                    return String.Empty;
                }
            }
            return _appState.Config.LightSettings.Hue.HueApiKey;
        }

        public async Task<string> FindBridge()
        {
            try
            {
                HttpBridgeLocator locator = new HttpBridgeLocator();
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
                    _client = new LocalHueApi(_appState.Config.LightSettings.Hue.HueIpAddress, _appState.Config.LightSettings.Hue.HueApiKey);
                }
                var lightsResponse = await _client.Light.GetAllAsync();
                return lightsResponse.Data;
            }
            catch (Exception e)
            {
                _logger.LogError(e, message: "Error Occurred Getting Lights");
                throw;
            }
        }

        public async Task<IEnumerable<GroupedLight>> GetGroups()
        {
            try
            {
                if (_client == null)
                {
                    _client = new LocalHueApi(_appState.Config.LightSettings.Hue.HueIpAddress, _appState.Config.LightSettings.Hue.HueApiKey);
                }
                var groupsResponse = await _client.GroupedLight.GetAllAsync();
                return groupsResponse.Data;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Occurred Getting Groups");
                throw;
            }
        }

        private async Task<(string color, UpdateLight command, bool returnFunc)> Handle(string presence, string lightId)
        {
            var props = _appState.Config.LightSettings.Hue.Statuses.GetType().GetProperties().ToList();

            if (_appState.Config.LightSettings.Hue.UseActivityStatus)
            {
                props = props.Where(a => a.Name.ToLower().StartsWith("activity")).ToList();
            }
            else
            {
                props = props.Where(a => a.Name.ToLower().StartsWith("availability")).ToList();
            }

            string color = "";
            string message;
            var command = new UpdateLight();

            if (presence.Contains('#'))
            {
                // provided presence is actually a custom color
                color = presence;
                command.TurnOn();
                return (color, command, false);
            }

            foreach (var prop in props)
            {
                if (presence == prop.Name.Replace("Status", "").Replace("Availability", "").Replace("Activity", ""))
                {
                    var value = (AvailabilityStatus)prop.GetValue(_appState.Config.LightSettings.Hue.Statuses);

                    if (!value.Disabled)
                    {
                        command.TurnOn();
                        color = value.Color;
                        return (color, command, false);
                    }
                    else
                    {
                        command.TurnOff();

                        if (lightId.Contains("group_id:"))
                        {
                            var groupCommand = new UpdateGroupedLight();
                            groupCommand.TurnOff();
                            await _client.UpdateGroupedLightAsync(Guid.Parse(lightId.Replace("group_id:", "")), groupCommand);
                        }
                        else
                        {
                            await _client.UpdateLightAsync(Guid.Parse(lightId.Replace("id:", "")), command);
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

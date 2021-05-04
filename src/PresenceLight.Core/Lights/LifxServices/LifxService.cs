using System;
using LifxCloud.NET;
using LifxCloud.NET.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace PresenceLight.Core
{
    public class LIFXService
    {
        private BaseConfig _options;
        private LifxCloudClient _client;
        private readonly ILogger<LIFXService> _logger;
        MediatR.IMediator _mediator;

        public LIFXService(Microsoft.Extensions.Options.IOptionsMonitor<BaseConfig> optionsAccessor, MediatR.IMediator mediator, ILogger<LIFXService> logger)
        {
            _logger = logger;
            _options = optionsAccessor.CurrentValue;
            _mediator = mediator;
        }

        public void Initialize(BaseConfig options)
        {
            _options = options;
        }

        public async Task<List<Light>> GetAllLights(string apiKey = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(apiKey))
                {
                    _options.LightSettings.LIFX.LIFXApiKey = apiKey;
                }

                if (!_options.LightSettings.LIFX.IsEnabled || string.IsNullOrEmpty(_options.LightSettings.LIFX.LIFXApiKey))
                {
                    return new List<Light>();
                }

                _client = await LifxCloudClient.CreateAsync(_options.LightSettings.LIFX.LIFXApiKey);
                return await _client.ListLights(Selector.All);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Getting Lights");
                throw;
            }
        }

        public async Task<List<Group>> GetAllGroups(string apiKey = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(apiKey))
                {
                    _options.LightSettings.LIFX.LIFXApiKey = apiKey;
                }
                if (!_options.LightSettings.LIFX.IsEnabled || string.IsNullOrEmpty(_options.LightSettings.LIFX.LIFXApiKey))
                {
                    return new List<Group>();
                }

                _client = await LifxCloudClient.CreateAsync(_options.LightSettings.LIFX.LIFXApiKey);
                return await _client.ListGroups(Selector.All);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Getting Groups");
                throw;
            }
        }

        public async Task SetColor(string availability, string activity, string lightId, string apiKey = null)
        {
            if (string.IsNullOrEmpty(lightId))
            {
                _logger.LogInformation("Selected LIFX Light Not Specified");
                return;
            }

            Selector selector = null;

            if (!lightId.Contains("group"))
            {
                selector = new Selector.LightId(lightId.Replace("id:", ""));
            }
            else
            {
                selector = new Selector.GroupId(lightId.Replace("group_id:", ""));
            }

            if (!string.IsNullOrEmpty(apiKey))
            {
                _options.LightSettings.LIFX.LIFXApiKey = apiKey;
            }
            if (!_options.LightSettings.LIFX.IsEnabled || string.IsNullOrEmpty(_options.LightSettings.LIFX.LIFXApiKey))
            {
                return;
            }

            try
            {
                _client = await LifxCloudClient.CreateAsync(_options.LightSettings.LIFX.LIFXApiKey);

                var o = await Handle(_options.LightSettings.LIFX.UseActivityStatus ? activity : availability, lightId);

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

                if (availability == "Off")
                {
                    _logger.LogInformation($"Turning LIFX Light {lightId} Off - LIFXService:SetColor");
                    command.Power = PowerState.Off;
                    var result = await _client.SetState(selector, command);
                    return;
                }

                if (_options.LightSettings.UseDefaultBrightness)
                {
                    if (_options.LightSettings.DefaultBrightness == 0)
                    {
                        command.Power = PowerState.Off;
                    }
                    else
                    {
                        command.Power = PowerState.On;
                        command.Brightness = Convert.ToDouble(_options.LightSettings.DefaultBrightness) / 100;
                        command.Duration = 0;
                    }
                }
                else
                {
                    if (_options.LightSettings.LIFX.Brightness == 0)
                    {
                        command.Power = PowerState.Off;
                    }
                    else
                    {
                        command.Power = PowerState.On;
                        command.Brightness = Convert.ToDouble(_options.LightSettings.DefaultBrightness) / 100;
                        command.Duration = 0;
                    }
                }
                command.Color = color;
                await _client.SetState(selector, command);

                message = $"Setting LIFX Light {lightId} to {color}";
                _logger.LogInformation(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Occured Setting Color");
                throw;
            }
        }

        private async Task<(string color, SetStateRequest command, bool returnFunc)> Handle(string presence, string lightId)
        {
            var props = _options.LightSettings.LIFX.Statuses.GetType().GetProperties().ToList();

            if (_options.LightSettings.LIFX.UseActivityStatus)
            {
                props = props.Where(a => a.Name.ToLower().StartsWith("activity")).ToList();
            }
            else
            {
                props = props.Where(a => a.Name.ToLower().StartsWith("availability")).ToList();
            }

            string color = "";
            string message;
            var command = new SetStateRequest();

            if (presence.Contains("#"))
            {
                // provided presence is actually a custom color
                color = presence;
                command.Power = PowerState.On;
                return (color, command, false);
            }

            foreach (var prop in props)
            {
                if (presence == prop.Name.Replace("Status", "").Replace("Availability", "").Replace("Activity", ""))
                {
                    var value = (AvailabilityStatus)prop.GetValue(_options.LightSettings.LIFX.Statuses);

                    if (!value.Disabled)
                    {
                        command.Power = PowerState.On;
                        color = value.Colour;
                        return (color, command, false);
                    }
                    else
                    {
                        command.Power = PowerState.Off;


                        Selector selector = null;

                        if (!lightId.Contains("group"))
                        {
                            selector = new Selector.LightId(lightId.Replace("id:", ""));
                        }
                        else
                        {
                            selector = new Selector.GroupId(lightId.Replace("group_id:", ""));
                        }

                        await _client.SetState(selector, command);

                        message = $"Turning LIFX Light {lightId} Off";
                        _logger.LogInformation(message);
                        return (color, command, true);
                    }
                }
            }
            return (color, command, false);
        }
    }
}

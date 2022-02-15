using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenWiz;
using System.Net;
using Microsoft.Extensions.Logging;
using Q42.HueApi.ColorConverters;
using PresenceLight.Core.WizServices;

namespace PresenceLight.Core
{
    public interface IWizService
    {
        Task<IEnumerable<WizLight>> GetLights();

        Task SetColor(string availability, string activity, string lightId);
    }
    public class WizService : IWizService
    {
        private AppState _appState;
        private readonly ILogger<WizService> _logger;
        MediatR.IMediator _mediator;
        public WizService(AppState appState, MediatR.IMediator mediator, ILogger<WizService> logger)
        {
            _mediator = mediator;
            _logger = logger;
            _appState = appState;
        }

        public WizService(AppState appState)
        {
            _appState = appState;
        }

        public async Task<IEnumerable<WizLight>> GetLights()
        {
            var lights = GetHomeIds();
            List<WizLight> wizLights = new List<WizLight>();
            foreach (var light in lights)
            {
                wizLights.Add(new WizLight
                {
                    LightName = light.LightName,
                    MacAddress = light.MacAddress
                });
            }

            return wizLights;

        }

        public async Task SetColor(string availability, string activity, string lightId)
        {
            if (string.IsNullOrEmpty(lightId))
            {
                _logger.LogInformation("Selected Wiz Light Not Specified");
                return;
            }

            try
            {
                var o = Handle(_appState.Config.LightSettings.Wiz.UseActivityStatus ? activity : availability, lightId);

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

                var rgb = new RGBColor(color);

                command.R = Convert.ToInt32(rgb.R);
                command.B = Convert.ToInt32(rgb.B);
                command.G = Convert.ToInt32(rgb.G);

                if (availability == "Off")
                {
                    command.State = false;

                    UpdateLight(command, lightId);
                    message = $"Turning Wiz Light {lightId} Off";
                    _logger.LogInformation(message);
                    return;
                }

                if (_appState.Config.LightSettings.UseDefaultBrightness)
                {
                    if (_appState.Config.LightSettings.DefaultBrightness == 0)
                    {
                        command.State = false;
                    }
                    else
                    {
                        command.State = true;
                        command.Dimming = _appState.Config.LightSettings.DefaultBrightness;
                        command.Speed = 0;
                    }
                }
                else
                {
                    if (_appState.Config.LightSettings.Wiz.Brightness == 0)
                    {
                        command.State = false;
                    }
                    else
                    {
                        command.State = true;
                        command.Dimming = _appState.Config.LightSettings.Wiz.Brightness;
                        command.Speed = 0;
                    }
                }

                UpdateLight(command, lightId);

                message = $"Setting Wiz Light {lightId} to {color}";
                _logger.LogInformation(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Occurred Setting Color");
                throw;
            }
        }

        private IEnumerable<(string LightName, string MacAddress)> GetHomeIds()
        {
            WizSocket socket = new WizSocket();
            socket.GetSocket().EnableBroadcast = true; // This will enable sending to the broadcast address

            WizHandle handle = new WizHandle("000000000000", IPAddress.Broadcast); // MAC doesn't matter here

            WizState state = WizState.MakeGetSystemConfig();

            socket.GetSocket().ReceiveTimeout = 1000; // This will prevent the demo from running indefinitely
            socket.SendTo(state, handle);

            List<(string LightName, string MacAddress)> homeIds = new List<(string LightName, string MacAddress)>();

            // You won't easily get an IP address here, but this will list all Home IDs on the network.
            while (true)
            {
                state = socket.ReceiveFrom(handle);
                if (state.Result.HomeId != null)
                {
                    Console.WriteLine($"Home ID for light {state.Result.Mac} = {state.Result.HomeId}");

                    (string LightName, string MacAddress) result = (state.Result.ModuleName, state.Result.Mac);
                    homeIds.Add(result);
                }
                break;
            }

            return homeIds;
        }

        private (string color, WizParams command, bool returnFunc) Handle(string presence, string lightId)
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

            var command = new WizParams();

            if (presence.Contains("#"))
            {
                // provided presence is actually a custom color
                color = presence;
                command.State = true;
                return (color, command, false);
            }

            foreach (var prop in props)
            {
                if (presence == prop.Name.Replace("Status", "").Replace("Availability", "").Replace("Activity", ""))
                {
                    var value = (AvailabilityStatus)prop.GetValue(_appState.Config.LightSettings.Hue.Statuses);

                    if (!value.Disabled)
                    {
                        command.State = true;
                        color = value.Colour;
                        return (color, command, false);
                    }
                    else
                    {
                        command.State = false;
                        UpdateLight(command, lightId);
                        message = $"Turning Hue Light {lightId} Off";
                        _logger.LogInformation(message);
                        return (color, command, true);
                    }
                }
            }
            return (color, command, false);
        }
        private WizResult UpdateLight(WizParams wizParams, string lightId)
        {
            WizSocket socket = new WizSocket();
            socket.GetSocket().EnableBroadcast = true; // This will enable sending to the broadcast address
            socket.GetSocket().ReceiveTimeout = 1000; // This will prevent the demo from running indefinitely
            WizHandle handle = new WizHandle(lightId, IPAddress.Broadcast); // MAC doesn't matter here

            WizState state = new WizState
            {
                Method = WizMethod.setPilot,
                Params = wizParams
            };

            socket.SendTo(state, handle);

            WizResult pilot;
            while (true)
            {
                state = socket.ReceiveFrom(handle);
                pilot = state.Result;
                break;
            }

            return pilot;
        }
    }
}

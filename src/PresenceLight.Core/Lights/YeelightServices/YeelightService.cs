using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Threading;
using YeelightAPI;
using System.Drawing;
using Q42.HueApi.ColorConverters;
using Microsoft.Extensions.Logging;

namespace PresenceLight.Core
{
    public interface IYeelightService
    {
        Task SetColor(string availability, string activity, string lightId);
        Task<DeviceGroup> FindLights();
    }
    public class YeelightService : IYeelightService
    {
        private BaseConfig _options;

        private MediatR.IMediator _mediator;
        private DeviceGroup deviceGroup;
        private readonly ILogger<YeelightService> _logger;

        public YeelightService(IOptionsMonitor<BaseConfig> optionsAccessor, ILogger<YeelightService> logger, MediatR.IMediator mediator)
        {
            _logger = logger;
            _options = optionsAccessor.CurrentValue;
            _mediator = mediator;
        }

        public void Initialize(BaseConfig options)
        {
            _options = options;
        }

        public async Task SetColor(string availability, string activity, string lightId)
        {
            string message = "";

            if (string.IsNullOrEmpty(lightId))
            {
                _logger.LogInformation("Selected Yeelight Light Not Specified");
                return;
            }

            var o = await Handle(_options.LightSettings.Yeelight.UseActivityStatus ? activity : availability, lightId);

            if (o.returnFunc)
            {
                return;
            }

            if (o.device == null)
            {
                message = $"Yeelight Device {lightId} Not Found";
                _logger.LogError(message);
                throw new ArgumentOutOfRangeException(nameof(lightId), message);
            }

            o.device.OnNotificationReceived += Device_OnNotificationReceived;
            o.device.OnError += Device_OnError;

            if (!await o.device.Connect())
            {
                message = $"Unable to Connect to Yeelight Device {lightId}";
                _logger.LogError(message);
                throw new ArgumentOutOfRangeException(nameof(lightId), message);
            }

            try
            {
                var color = o.color.Replace("#", "");

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
                    await o.device.TurnOff();

                    message = $"Turning Yeelight Light {lightId} Off";
                    _logger.LogInformation(message);
                    return;
                }

                if (_options.LightSettings.UseDefaultBrightness)
                {
                    if (_options.LightSettings.DefaultBrightness == 0)
                    {
                        await o.device.TurnOff();
                    }
                    else
                    {
                        await o.device.TurnOn();
                        await o.device.SetBrightness(Convert.ToInt32(_options.LightSettings.DefaultBrightness));
                    }
                }
                else
                {
                    if (_options.LightSettings.Hue.Brightness == 0)
                    {
                        await o.device.TurnOff();
                    }
                    else
                    {
                        await o.device.TurnOn();
                        await o.device.SetBrightness(Convert.ToInt32(_options.LightSettings.Yeelight.Brightness));
                    }
                }

                var rgb = new RGBColor(color);
                await o.device.SetRGBColor((int)rgb.R, (int)rgb.G, (int)rgb.B);
                return;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Occured Finding Lights");
                throw;
            }
        }
        private void Device_OnError(object sender, UnhandledExceptionEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void Device_OnNotificationReceived(object sender, NotificationReceivedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public async Task<DeviceGroup> FindLights()
        {
            try
            {
                IEnumerable<Device> devices = await DeviceLocator.DiscoverAsync();
                this.deviceGroup = new DeviceGroup(devices);
                return this.deviceGroup;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Occured Finding Lights");
                throw;
            }
        }

        private async Task<(string color, Device device, bool returnFunc)> Handle(string presence, string lightId)
        {
            var props = _options.LightSettings.Yeelight.Statuses.GetType().GetProperties().ToList();

            if (_options.LightSettings.Yeelight.UseActivityStatus)
            {
                props = props.Where(a => a.Name.ToLower().StartsWith("activity")).ToList();
            }
            else
            {
                props = props.Where(a => a.Name.ToLower().StartsWith("availability")).ToList();
            }

            string color = "";
            string message;
            var device = this.deviceGroup.FirstOrDefault(x => x.Id == lightId);

            if (device != null)
            {
                device.OnNotificationReceived += Device_OnNotificationReceived;
                device.OnError += Device_OnError;

                await device.Connect();

                if (presence.Contains("#"))
                {
                    // provided presence is actually a custom color
                    color = presence;
                    await device.TurnOn();
                    return (color, device, false);
                }

                foreach (var prop in props)
                {
                    if (presence == prop.Name.Replace("Status", "").Replace("Availability", "").Replace("Activity", ""))
                    {
                        var value = (AvailabilityStatus)prop.GetValue(_options.LightSettings.Yeelight.Statuses);

                        if (!value.Disabled)
                        {
                            await device.TurnOn();
                            color = value.Colour;
                            return (color, device, false);
                        }
                        else
                        {
                            await device.TurnOff();

                            message = $"Turning Yeelight Light {lightId} Off";
                            _logger.LogInformation(message);
                            return (color, device, true);
                        }
                    }
                }
            }
            return (color, device, false);
        }
    }
}



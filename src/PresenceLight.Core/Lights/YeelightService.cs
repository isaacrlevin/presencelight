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
        Task SetColor(string availability, string lightId);
        Task<DeviceGroup> FindLights();
    }
    public class YeelightService : IYeelightService
    {
        private readonly BaseConfig _options;

        private DeviceGroup deviceGroup;
        private readonly ILogger<YeelightService> _logger;

        public YeelightService(IOptionsMonitor<BaseConfig> optionsAccessor, ILogger<YeelightService> logger)
        {
            _logger = logger;
            _options = optionsAccessor.CurrentValue;
        }

        public YeelightService(BaseConfig options)
        {
            _options = options;
        }

        public async Task SetColor(string availability, string lightId)
        {
            string message = "";

            if (string.IsNullOrEmpty(lightId))
            {
                Helpers.AppendLogger(_logger, $"Yeelight Selected Light Id {lightId} Invalid", new ArgumentOutOfRangeException());
                throw new ArgumentOutOfRangeException($"Yeelight Selected Light Id {lightId} Invalid");
            }

            var device = this.deviceGroup.FirstOrDefault(x => x.Id == lightId);

            if (device == null)
            {
                message = $"Yeelight Device {lightId} Not Found";
                Helpers.AppendLogger(_logger, message, new ArgumentOutOfRangeException());
                throw new ArgumentOutOfRangeException(message);
            }

            device.OnNotificationReceived += Device_OnNotificationReceived;
            device.OnError += Device_OnError;

            if (!await device.Connect())
            {
                message = $"Unable to Connect to Yeelight Device {lightId}";
                Helpers.AppendLogger(_logger, message, new ArgumentOutOfRangeException());
                throw new ArgumentOutOfRangeException(message);
            }

            try
            {
                string color = "";

                switch (availability)
                {
                    case "Available":
                        if (!_options.LightSettings.Yeelight.AvailableStatus.Disabled)
                        {

                            color = _options.LightSettings.Yeelight.AvailableStatus.Colour;
                        }
                        else
                        {
                            message = $"Turning Yeelight Light {lightId} Off";
                            Helpers.AppendLogger(_logger, message);
                            await device.SetPower(false);
                            return;
                        }
                        break;
                    case "Busy":
                        if (!_options.LightSettings.Yeelight.BusyStatus.Disabled)
                        {
                            color = _options.LightSettings.Yeelight.BusyStatus.Colour;
                        }
                        else
                        {
                            message = $"Turning Yeelight Light {lightId} Off";
                            Helpers.AppendLogger(_logger, message);
                            await device.SetPower(false);
                            return;
                        }
                        break;
                    case "BeRightBack":
                        if (!_options.LightSettings.Yeelight.BeRightBackStatus.Disabled)
                        {
                            color = _options.LightSettings.Yeelight.BeRightBackStatus.Colour;
                        }
                        else
                        {
                            message = $"Turning Yeelight Light {lightId} Off";
                            Helpers.AppendLogger(_logger, message);
                            await device.SetPower(false);
                            return;
                        }
                        break;
                    case "Away":
                        if (!_options.LightSettings.Yeelight.AwayStatus.Disabled)
                        {
                            color = _options.LightSettings.Yeelight.AwayStatus.Colour;
                        }
                        else
                        {
                            message = $"Turning Yeelight Light {lightId} Off";
                            Helpers.AppendLogger(_logger, message);
                            await device.SetPower(false);
                            return;
                        }
                        break;
                    case "DoNotDisturb":
                        if (!_options.LightSettings.Yeelight.DoNotDisturbStatus.Disabled)
                        {
                            color = _options.LightSettings.Yeelight.DoNotDisturbStatus.Colour;
                        }
                        else
                        {
                            message = $"Turning Yeelight Light {lightId} Off";
                            Helpers.AppendLogger(_logger, message);
                            await device.SetPower(false);
                            return;
                        }
                        break;
                    case "Offline":
                        if (!_options.LightSettings.Yeelight.OfflineStatus.Disabled)
                        {
                            color = _options.LightSettings.Yeelight.OfflineStatus.Colour;
                        }
                        else
                        {
                            message = $"Turning Yeelight Light {lightId} Off";
                            Helpers.AppendLogger(_logger, message);
                            await device.SetPower(false);
                            return;
                        }
                        break;
                    case "Off":
                        if (!_options.LightSettings.Yeelight.OffStatus.Disabled)
                        {
                            color = _options.LightSettings.Yeelight.OffStatus.Colour;
                        }
                        else
                        {
                            message = $"Turning Yeelight Light {lightId} Off";
                            Helpers.AppendLogger(_logger, message);
                            await device.SetPower(false);
                            return;
                        }
                        break;
                    default:
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

                if (_options.LightSettings.UseDefaultBrightness)
                {
                    if (_options.LightSettings.DefaultBrightness == 0)
                    {
                        await device.TurnOff();
                    }
                    else
                    {
                        await device.TurnOn();
                        await device.SetBrightness(Convert.ToInt32(_options.LightSettings.DefaultBrightness));
                    }
                }
                else
                {
                    if (_options.LightSettings.Hue.HueBrightness == 0)
                    {
                        await device.TurnOff();
                    }
                    else
                    {
                        await device.TurnOn();
                        await device.SetBrightness(Convert.ToInt32(_options.LightSettings.Yeelight.YeelightBrightness));
                    }
                }

                var rgb = new RGBColor(availability);
                await device.SetRGBColor((int)rgb.R, (int)rgb.G, (int)rgb.B);
                return;
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error Occured Finding Lights", e);
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
                Helpers.AppendLogger(_logger, "Error Occured Finding Lights", e);
                throw;
            }
        }
    }
}


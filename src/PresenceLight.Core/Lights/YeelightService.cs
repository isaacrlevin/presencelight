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

namespace PresenceLight.Core
{
    public interface IYeelightService
    {
        Task SetColor(string availability, string lightId);
        Task<DeviceGroup> FindLights();
    }
    public class YeelightService : IYeelightService
    {
        private readonly ConfigWrapper _options;

        private DeviceGroup deviceGroup;

        public YeelightService(IOptionsMonitor<ConfigWrapper> optionsAccessor)
        {
            _options = optionsAccessor.CurrentValue;
        }

        public YeelightService(ConfigWrapper options)
        {
            _options = options;
        }

        public async Task SetColor(string availability, string lightId)
        {
            var device = this.deviceGroup.FirstOrDefault(x => x.Id == lightId);

            if (device == null)
            {
                return;
            }

            device.OnNotificationReceived += Device_OnNotificationReceived;
            device.OnError += Device_OnError;

            if (!await device.Connect())
            {
                return;
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

            switch (availability)
            {
                case "Available":
                    await device.SetRGBColor(0, 63, 21);
                    break;
                case "Busy":
                    await device.SetRGBColor(255, 51, 0);
                    break;
                case "BeRightBack":
                    await device.SetRGBColor(255, 255, 0);
                    break;
                case "Away":
                    await device.SetRGBColor(255, 255, 0);
                    break;
                case "DoNotDisturb":
                    await device.SetRGBColor(128, 0, 0);
                    break;
                case "Offline":
                    await device.SetRGBColor(255, 255, 255);
                    break;
                case "Off":
                    await device.SetRGBColor(255, 255, 255);
                    break;
                default:
                    var color = new RGBColor(availability);
                    await device.SetRGBColor((int)color.R, (int)color.G, (int)color.B);
                    break;
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
                List<Device> devices = await DeviceLocator.Discover();
                this.deviceGroup = new DeviceGroup(devices);
                return this.deviceGroup;
            }
            catch
            {
                return null;
            }
        }
    }
}


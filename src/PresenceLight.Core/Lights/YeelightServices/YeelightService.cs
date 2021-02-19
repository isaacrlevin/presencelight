﻿using System;
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
                _logger.LogError($"Yeelight Selected Light Id {lightId} Invalid");
                throw new ArgumentOutOfRangeException(nameof(lightId), $"Yeelight Selected Light Id {lightId} Invalid");
            }

            bool useWorkingHours = await _mediator.Send(new WorkingHoursServices.UseWorkingHoursCommand());
            bool IsInWorkingHours = await _mediator.Send(new WorkingHoursServices.IsInWorkingHoursCommand());

            if (!useWorkingHours || (useWorkingHours && IsInWorkingHours))
            {
                var device = this.deviceGroup.FirstOrDefault(x => x.Id == lightId);

                if (device == null)
                {
                    message = $"Yeelight Device {lightId} Not Found";
                    _logger.LogError(message);
                    throw new ArgumentOutOfRangeException(nameof(lightId), message);
                }

                device.OnNotificationReceived += Device_OnNotificationReceived;
                device.OnError += Device_OnError;

                if (!await device.Connect())
                {
                    message = $"Unable to Connect to Yeelight Device {lightId}";
                    _logger.LogError(message);
                    throw new ArgumentOutOfRangeException(nameof(lightId), message);
                }

                try
                {
                    string color = "";

                    if (_options.LightSettings.Yeelight.UseActivityStatus)
                    { }
                    else
                    {
                        switch (availability)
                        {
                            case "Available":
                                if (!_options.LightSettings.Yeelight.Statuses.AvailabilityAvailableStatus.Disabled)
                                {

                                    color = _options.LightSettings.Yeelight.Statuses.AvailabilityAvailableStatus.Colour;
                                }
                                else
                                {
                                    message = $"Turning Yeelight Light {lightId} Off";
                                    _logger.LogInformation(message);
                                    await device.SetPower(false);
                                    return;
                                }
                                break;
                            case "Busy":
                                if (!_options.LightSettings.Yeelight.Statuses.AvailabilityBusyStatus.Disabled)
                                {
                                    color = _options.LightSettings.Yeelight.Statuses.AvailabilityBusyStatus.Colour;
                                }
                                else
                                {
                                    message = $"Turning Yeelight Light {lightId} Off";
                                    _logger.LogInformation(message);
                                    await device.SetPower(false);
                                    return;
                                }
                                break;
                            case "BeRightBack":
                                if (!_options.LightSettings.Yeelight.Statuses.AvailabilityBeRightBackStatus.Disabled)
                                {
                                    color = _options.LightSettings.Yeelight.Statuses.AvailabilityBeRightBackStatus.Colour;
                                }
                                else
                                {
                                    message = $"Turning Yeelight Light {lightId} Off";
                                    _logger.LogInformation(message);
                                    await device.SetPower(false);
                                    return;
                                }
                                break;
                            case "Away":
                                if (!_options.LightSettings.Yeelight.Statuses.AvailabilityAwayStatus.Disabled)
                                {
                                    color = _options.LightSettings.Yeelight.Statuses.AvailabilityAwayStatus.Colour;
                                }
                                else
                                {
                                    message = $"Turning Yeelight Light {lightId} Off";
                                    _logger.LogInformation(message);
                                    await device.SetPower(false);
                                    return;
                                }
                                break;
                            case "DoNotDisturb":
                                if (!_options.LightSettings.Yeelight.Statuses.AvailabilityDoNotDisturbStatus.Disabled)
                                {
                                    color = _options.LightSettings.Yeelight.Statuses.AvailabilityDoNotDisturbStatus.Colour;
                                }
                                else
                                {
                                    message = $"Turning Yeelight Light {lightId} Off";
                                    _logger.LogInformation(message);
                                    await device.SetPower(false);
                                    return;
                                }
                                break;
                            case "Offline":
                                if (!_options.LightSettings.Yeelight.Statuses.AvailabilityOfflineStatus.Disabled)
                                {
                                    color = _options.LightSettings.Yeelight.Statuses.AvailabilityOfflineStatus.Colour;
                                }
                                else
                                {
                                    message = $"Turning Yeelight Light {lightId} Off";
                                    _logger.LogInformation(message);
                                    await device.SetPower(false);
                                    return;
                                }
                                break;
                            case "Off":
                                if (!_options.LightSettings.Yeelight.Statuses.AvailabilityOffStatus.Disabled)
                                {
                                    color = _options.LightSettings.Yeelight.Statuses.AvailabilityOffStatus.Colour;
                                }
                                else
                                {
                                    message = $"Turning Yeelight Light {lightId} Off";
                                    _logger.LogInformation(message);
                                    await device.SetPower(false);
                                    return;
                                }
                                break;
                            default:
                                color = availability;
                                break;
                        }
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
                        if (_options.LightSettings.Hue.Brightness == 0)
                        {
                            await device.TurnOff();
                        }
                        else
                        {
                            await device.TurnOn();
                            await device.SetBrightness(Convert.ToInt32(_options.LightSettings.Yeelight.Brightness));
                        }
                    }

                    var rgb = new RGBColor(color);
                    await device.SetRGBColor((int)rgb.R, (int)rgb.G, (int)rgb.B);
                    return;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error Occured Finding Lights");
                    throw;
                }
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
    }
}



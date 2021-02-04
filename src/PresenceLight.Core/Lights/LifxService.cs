using System;
using LifxCloud.NET;
using LifxCloud.NET.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PresenceLight.Core
{
    public class LIFXService
    {
        private BaseConfig _options;
        private LifxCloudClient _client;
        private readonly ILogger<LIFXService> _logger;
        private readonly IWorkingHoursService _workingHoursService;

        public LIFXService(Microsoft.Extensions.Options.IOptionsMonitor<BaseConfig> optionsAccessor, ILogger<LIFXService> logger, IWorkingHoursService workingHoursService)
        {
            _workingHoursService = workingHoursService;
            _logger = logger;
            _options = optionsAccessor.CurrentValue;
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
                Helpers.AppendLogger(_logger, "Error Getting Lights", e);
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
                Helpers.AppendLogger(_logger, "Error Getting Groups", e);
                throw;
            }
        }

        public async Task SetColor(string availability, string activity, string lightId, string apiKey = null)
        {
            if (string.IsNullOrEmpty(lightId))
            {
                throw new ArgumentNullException("Selected LIFX Light Not Specified");
            }
            Selector selector = new Selector.LightId(lightId);

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
                if (!_workingHoursService.UseWorkingHours || (_workingHoursService.UseWorkingHours && _workingHoursService.IsInWorkingHours))
                {
                    _client = await LifxCloudClient.CreateAsync(_options.LightSettings.LIFX.LIFXApiKey);
                    string color = "";
                    switch (availability)
                    {
                        case "Available":
                            if (!_options.LightSettings.LIFX.Statuses.AvailabilityAvailableStatus.Disabled)
                            {
                                color = $"{_options.LightSettings.LIFX.Statuses.AvailabilityAvailableStatus.Colour.ToString()}";
                            }
                            else
                            {
                                _logger.LogInformation($"Turning LIFX Light {lightId} Off - LIFXService:SetColor");
                                var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                                {
                                    Power = PowerState.Off
                                });
                                return;
                            }
                            break;
                        case "Busy":
                            if (!_options.LightSettings.LIFX.Statuses.AvailabilityBusyStatus.Disabled)
                            {
                                color = $"{_options.LightSettings.LIFX.Statuses.AvailabilityBusyStatus.Colour.ToString()}";
                            }
                            else
                            {
                                _logger.LogInformation($"Turning LIFX Light {lightId} Off - LIFXService:SetColor");
                                var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                                {
                                    Power = PowerState.Off
                                });
                                return;
                            }
                            break;
                        case "BeRightBack":
                            if (!_options.LightSettings.LIFX.Statuses.AvailabilityBeRightBackStatus.Disabled)
                            {
                                color = $"{_options.LightSettings.LIFX.Statuses.AvailabilityBeRightBackStatus.Colour.ToString()}";
                            }
                            else
                            {
                                _logger.LogInformation($"Turning LIFX Light {lightId} Off - LIFXService:SetColor");
                                var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                                {
                                    Power = PowerState.Off
                                });
                                return;
                            }
                            break;
                        case "Away":
                            if (!_options.LightSettings.LIFX.Statuses.AvailabilityAwayStatus.Disabled)
                            {
                                color = $"{_options.LightSettings.LIFX.Statuses.AvailabilityAwayStatus.Colour.ToString()}";
                            }
                            else
                            {
                                _logger.LogInformation($"Turning LIFX Light {lightId} Off - LIFXService:SetColor");
                                var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                                {
                                    Power = PowerState.Off
                                });
                                return;
                            }
                            break;
                        case "DoNotDisturb":
                            if (!_options.LightSettings.LIFX.Statuses.AvailabilityDoNotDisturbStatus.Disabled)
                            {
                                color = $"{_options.LightSettings.LIFX.Statuses.AvailabilityDoNotDisturbStatus.Colour.ToString()}";
                            }
                            else
                            {
                                _logger.LogInformation($"Turning LIFX Light {lightId} Off - LIFXService:SetColor");
                                var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                                {
                                    Power = PowerState.Off
                                });
                                return;
                            }
                            break;
                        case "Offline":
                            if (!_options.LightSettings.LIFX.Statuses.AvailabilityOfflineStatus.Disabled)
                            {
                                color = $"{_options.LightSettings.LIFX.Statuses.AvailabilityOfflineStatus.Colour.ToString()}";
                            }
                            else
                            {
                                _logger.LogInformation($"Turning LIFX Light {lightId} Off - LIFXService:SetColor");
                                var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                                {
                                    Power = PowerState.Off
                                });
                                return;
                            }
                            break;
                        case "Off":
                            if (!_options.LightSettings.LIFX.Statuses.AvailabilityOffStatus.Disabled)
                            {
                                color = $"{_options.LightSettings.LIFX.Statuses.AvailabilityOffStatus.Colour.ToString()}";
                            }
                            else
                            {
                                _logger.LogInformation($"Turning LIFX Light {lightId} Off - LIFXService:SetColor");
                                var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                                {
                                    Power = PowerState.Off
                                });
                                return;
                            }
                            break;
                        default:
                            color = $"{_options.LightSettings.LIFX.Statuses.AvailabilityOffStatus.Colour.ToString()}";
                            break;
                    }

                    if (color.Length == 9 && color.Contains("#"))
                    {
                        color = $"#{color.Substring(3)}";
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

                    if (availability == "Off")
                    {
                        _logger.LogInformation($"Turning LIFX Light {lightId} Off - LIFXService:SetColor");
                        var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                        {
                            Power = PowerState.Off
                        });
                        return;
                    }


                    if (_options.LightSettings.UseDefaultBrightness)
                    {
                        if (_options.LightSettings.DefaultBrightness == 0)
                        {
                            _logger.LogInformation($"Turning LIFX Light {lightId} Off - LIFXService:SetColor");
                            var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                            {
                                Power = PowerState.Off
                            });
                        }
                        else
                        {
                            string message = $"Setting LIFX Light {lightId} to {color}";
                            Helpers.AppendLogger(_logger, message);
                            var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                            {
                                Brightness = Convert.ToDouble(_options.LightSettings.DefaultBrightness) / 100,
                                Color = color,
                                Duration = 0
                            });
                        }
                    }
                    else
                    {
                        if (_options.LightSettings.LIFX.Brightness == 0)
                        {
                            _logger.LogInformation($"Turning LIFX Light {lightId} Off - LIFXService:SetColor");
                            var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                            {
                                Power = PowerState.Off
                            });
                        }
                        else
                        {
                            string message = $"Setting LIFX Light {lightId} to {color}";
                            Helpers.AppendLogger(_logger, message);
                            var result = await _client.SetState(selector, new LifxCloud.NET.Models.SetStateRequest
                            {
                                Brightness = Convert.ToDouble(_options.LightSettings.LIFX.Brightness) / 100,
                                Color = color,
                                Duration = 0
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error Occured Setting Color", e);
                throw;
            }
        }
    }
}

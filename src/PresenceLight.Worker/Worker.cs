using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using LifxCloud.NET.Models;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

using PresenceLight.Core;

namespace PresenceLight.Worker
{
    public class Worker : BackgroundService
    {
        private readonly BaseConfig Config;
        private readonly IHueService _hueService;
        private readonly IRemoteHueService _remoteHueService;
        private readonly AppState _appState;
        private readonly ILogger<Worker> _logger;
        private LIFXService _lifxService;
        private IYeelightService _yeelightService;
        private ICustomApiService _customApiService;
        private GraphServiceClient c;
        private IWorkingHoursService _workingHoursService;

        public Worker(IHueService hueService,
                      ILogger<Worker> logger,
                      IOptionsMonitor<BaseConfig> optionsAccessor,
                      AppState appState,
                      LIFXService lifxService,
                      IYeelightService yeelightService,
                      IWorkingHoursService workingHoursService,
                      IRemoteHueService remoteHueService,
                      ICustomApiService customApiService)
        {
            Config = optionsAccessor.CurrentValue;
            _workingHoursService = workingHoursService;
            _hueService = hueService;
            _remoteHueService = remoteHueService;
            _lifxService = lifxService;
            _yeelightService = yeelightService;
            _customApiService = customApiService;
            _logger = logger;
            _appState = appState;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_appState.IsUserAuthenticated)
                {
                    c = _appState.GraphServiceClient;
                    Helpers.AppendLogger(_logger, "User is Authenticated, starting worker");
                    try
                    {
                        await Run();
                    }
                    catch (Exception e)
                    {
                        Helpers.AppendLogger(_logger, "Exception occured restarting worker", e);
                    }
                }
                else
                {
                    Helpers.AppendLogger(_logger, "User is Not Authenticated, restarting worker");
                }
                await Task.Delay(1000, stoppingToken);
            }
        }


        private async Task Run()
        {
            try
            {
                var user = await GetUserInformation();
                var photo = await GetPhotoAsBase64Async();
                var presence = await GetPresence();
                _appState.SetUserInfo(user, photo, presence);

                await SetColor(_appState.Presence.Availability, _appState.Presence.Activity);
                await InteractWithLights();
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Exception occured in running worker", e);
                throw;
            }
        }

        private async Task InteractWithLights()
        {
            bool previousWorkingHours = false;
            while (_appState.IsUserAuthenticated)
            {
                try
                {
                    await Task.Delay(Convert.ToInt32(Config.LightSettings.PollingInterval * 1000)).ConfigureAwait(true);

                    bool touchLight = false;
                    string newColor = "";

                    if (Config.LightSettings.SyncLights)
                    {
                        if (!_workingHoursService.UseWorkingHours)
                        {
                            if (_appState.LightMode == "Graph")
                            {
                                touchLight = true;
                            }
                        }
                        else
                        {
                            if (_workingHoursService.IsInWorkingHours)
                            {
                                previousWorkingHours = _workingHoursService.IsInWorkingHours;
                                if (_appState.LightMode == "Graph")
                                {
                                    touchLight = true;
                                }
                            }
                            else
                            {
                                // check to see if working hours have passed
                                if (previousWorkingHours)
                                {
                                    switch (Config.LightSettings.HoursPassedStatus)
                                    {
                                        case "Keep":
                                            break;
                                        case "White":
                                            newColor = "Offline";
                                            break;
                                        case "Off":
                                            newColor = "Off";
                                            break;
                                        default:
                                            break;
                                    }
                                    touchLight = true;
                                }
                            }
                        }
                    }

                    if (touchLight)
                    {
                        switch (_appState.LightMode)
                        {
                            case "Graph":
                                Helpers.AppendLogger(_logger, "PresenceLight Running in Teams Mode");
                                _appState.Presence = await System.Threading.Tasks.Task.Run(() => GetPresence()).ConfigureAwait(true);

                                if (newColor == string.Empty)
                                {
                                    await SetColor(_appState.Presence.Availability, _appState.Presence.Activity).ConfigureAwait(true);
                                }
                                else
                                {
                                    await SetColor(newColor, newColor).ConfigureAwait(true);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Helpers.AppendLogger(_logger, "Error Occured", e);
                }
            }
        }

        private async Task<User> GetUserInformation()
        {
            try
            {
                var me = await c.Me.Request().GetAsync();
                Helpers.AppendLogger(_logger, $"User is {me.DisplayName}");
                return me;
            }
            catch (Exception ex)
            {
                Helpers.AppendLogger(_logger, "Exception getting me", ex);
                throw;
            }
        }

        private async Task<string> GetPhotoAsBase64Async()
        {
            try
            {
                var photoStream = await c.Me.Photo.Content.Request().GetAsync();
                var memoryStream = new MemoryStream();
                photoStream.CopyTo(memoryStream);

                var photoBytes = memoryStream.ToArray();
                var base64Photo = $"data:image/gif;base64,{Convert.ToBase64String(photoBytes)}";

                return base64Photo;
            }
            catch (Exception ex)
            {
                Helpers.AppendLogger(_logger, "Exception getting photo", ex);
                throw;
            }
        }

        private async Task<Presence> GetPresence()
        {
            try
            {
                var presence = await c.Me.Presence.Request().GetAsync();

                var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

                Helpers.AppendLogger(_logger, $"Presence is {presence.Availability}");
                return presence;
            }
            catch (Exception ex)
            {
                Helpers.AppendLogger(_logger, "Exception getting presence", ex);
                throw;
            }
        }

        private async Task SetColor(string color, string activity = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedItemId))
                {
                    if (Config.LightSettings.Hue.UseRemoteApi)
                    {
                        if (!string.IsNullOrEmpty(Config.LightSettings.Hue.RemoteBridgeId))
                        {
                            await _remoteHueService.SetColor(color, Config.LightSettings.Hue.SelectedItemId, Config.LightSettings.Hue.RemoteBridgeId).ConfigureAwait(true);
                        }
                    }
                    else
                    {
                        await _hueService.SetColor(color, activity, Config.LightSettings.Hue.SelectedItemId).ConfigureAwait(true);
                    }
                }

                if (Config.LightSettings.LIFX.IsEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey))
                {
                    await _lifxService.SetColor(color, activity, Config.LightSettings.LIFX.SelectedItemId).ConfigureAwait(true);
                }

                if (Config.LightSettings.Yeelight.IsEnabled && !string.IsNullOrEmpty(Config.LightSettings.Yeelight.SelectedItemId))
                {
                    await _yeelightService.SetColor(color, activity, Config.LightSettings.Yeelight.SelectedItemId).ConfigureAwait(true);
                }

                if (Config.LightSettings.CustomApi.IsEnabled)
                {
                    string response = await _customApiService.SetColor(color, activity).ConfigureAwait(true);
                }
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error Occured", e);
            }
        }
    }
}

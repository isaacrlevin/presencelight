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
        private readonly AppState _appState;
        private readonly ILogger<Worker> _logger;
        UserAuthService _userAuthService;
        private readonly GraphServiceClient _graphClient;
        private MediatR.IMediator _mediator;

        private IWorkingHoursService _workingHoursService;


        public Worker(ILogger<Worker> logger,
                      IOptionsMonitor<BaseConfig> optionsAccessor,
                      AppState appState,
                      UserAuthService userAuthService,
                     MediatR.IMediator mediator,
                      IWorkingHoursService workingHoursService)
        {
            Config = optionsAccessor.CurrentValue;
            _workingHoursService = workingHoursService;
            _mediator = mediator;
            _userAuthService = userAuthService;
            _logger = logger;
            _appState = appState;

            _graphClient = new GraphServiceClient(userAuthService);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (await _userAuthService.IsUserAuthenticated())
                {
                    _logger.LogInformation("User is Authenticated, starting worker");
                    try
                    {
                        await Run();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Exception occured restarting worker");
                    }
                }
                else
                {
                    _logger.LogInformation("User is Not Authenticated, restarting worker");
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

                //Attach properties to all logging within this context..
                using (Serilog.Context.LogContext.PushProperty("Availability", presence.Availability))
                using (Serilog.Context.LogContext.PushProperty("Activity", presence.Activity))
                {
                    _appState.SetUserInfo(user, photo, presence);
                    _appState.SetUserInfo(user, photo, presence);

                    await SetColor(_appState.Presence.Availability, _appState.Presence.Activity);
                    await InteractWithLights();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occured in running worker");
                throw;
            }
        }

        private async Task InteractWithLights()
        {
            bool previousWorkingHours = false;
            while (await _userAuthService.IsUserAuthenticated())
            {

                bool useWorkingHours = await _mediator.Send(new Core.WorkingHoursServices.UseWorkingHoursCommand());
                bool IsInWorkingHours = await _mediator.Send(new Core.WorkingHoursServices.IsInWorkingHoursCommand());

                try
                {
                    await Task.Delay(Convert.ToInt32(Config.LightSettings.PollingInterval * 1000)).ConfigureAwait(true);

                    bool touchLight = false;
                    string newColor = "";

                    if (Config.LightSettings.SyncLights)
                    {
                        if (!useWorkingHours)
                        {
                            if (_appState.LightMode == "Graph")
                            {
                                touchLight = true;
                            }
                        }
                        else
                        {
                            if (IsInWorkingHours)
                            {
                                previousWorkingHours = IsInWorkingHours;
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
                                _logger.LogInformation("PresenceLight Running in Teams Mode");
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
                    _logger.LogError(e, "Error Occured Interacting with Lights");
                }
            }
        }

        private async Task<User> GetUserInformation()
        {
            try
            {
                var me = await _graphClient.Me.Request().GetAsync();
                _logger.LogInformation($"User is {me.DisplayName}");
                return me;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception getting me");
                throw;
            }
        }

        private async Task<string> GetPhotoAsBase64Async()
        {
            try
            {
                var photoStream = await _graphClient.Me.Photo.Content.Request().GetAsync();
                var memoryStream = new MemoryStream();
                photoStream.CopyTo(memoryStream);

                var photoBytes = memoryStream.ToArray();
                var base64Photo = $"data:image/gif;base64,{Convert.ToBase64String(photoBytes)}";

                return base64Photo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception getting photo");
                throw;
            }
        }

        private async Task<Presence> GetPresence()
        {
            try
            {
                var presence = await _graphClient.Me.Presence.Request().GetAsync();

                var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

                _logger.LogInformation($"Presence is {presence.Availability}");
                return presence;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception getting presence");
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
                            await _mediator.Send(new Core.RemoteHueServices.SetColorCommand
                            {
                                Availability = color,
                                LightId = Config.LightSettings.Hue.SelectedItemId,
                                BridgeId = Config.LightSettings.Hue.RemoteBridgeId
                            }).ConfigureAwait(true);
                        }
                    }
                    else
                    {
                        await _mediator.Send(new Core.HueServices.SetColorCommand() { Activity = activity, Availability = color, LightID = Config.LightSettings.Hue.SelectedItemId }).ConfigureAwait(true);

                    }
                }

                if (Config.LightSettings.LIFX.IsEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey))
                {
                    await _mediator.Send(new Core.LifxServices.SetColorCommand() { Availability = color, Activity = activity, LightId = Config.LightSettings.LIFX.SelectedItemId }).ConfigureAwait(true);
                }

                if (Config.LightSettings.Yeelight.IsEnabled && !string.IsNullOrEmpty(Config.LightSettings.Yeelight.SelectedItemId))
                {
                    await _mediator.Send(new PresenceLight.Core.YeelightServices.SetColorCommand { Activity = activity, Availability = color, LightId = Config.LightSettings.Yeelight.SelectedItemId }).ConfigureAwait(true);
                }

                if (Config.LightSettings.CustomApi.IsEnabled)
                {
                    string response = await _mediator.Send(new Core.CustomApiServices.SetColorCommand
                    {
                        Activity = activity,
                        Availability = color
                    });
                }

                if (Config.LightSettings.Wiz.IsEnabled)
                {
                    await _mediator.Send(new Core.WizServices.SetColorCommand
                    {
                        Activity = activity,
                        Availability = color,
                        LightID = Config.LightSettings.Wiz.SelectedItemId
                    });
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}

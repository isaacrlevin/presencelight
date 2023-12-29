using System.Text.RegularExpressions;

using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;

using PresenceLight.Core;

namespace PresenceLight.Web
{
    public class Worker : BackgroundService
    {
        private readonly AppState _appState;
        private readonly ILogger<Worker> _logger;
        LoginService loginService;
        
        private MediatR.IMediator _mediator;

        public Worker(ILogger<Worker> logger,
                      IOptionsMonitor<BaseConfig> optionsAccessor,
                      AppState appState,
                      LoginService _loginService,
                      MediatR.IMediator mediator)
        {
            _mediator = mediator;
            loginService = _loginService;
            _logger = logger;
            _appState = appState;
            _appState.Config = optionsAccessor.CurrentValue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (await loginService.IsUserAuthenticated())
                {
                    _logger.LogInformation("User is Authenticated, starting worker");
                    try
                    {
                        await Run();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Exception occurred restarting worker");
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
                if (!await _mediator.Send(new Core.GraphServices.GetIsInitializedCommand()))
                {
                    await _mediator.Send(new Core.GraphServices.InitializeCommand()
                    {
                    });
                    _appState.SignedIn = true;
                }

                var (user, presence) = await GetUserAndPresence();
                var photo = await GetPhotoAsBase64Async();
                
                //Attach properties to all logging within this context..
                using (Serilog.Context.LogContext.PushProperty("Availability", presence.Availability))
                using (Serilog.Context.LogContext.PushProperty("Activity", presence.Activity))
                {
                    _appState.SetUserInfo(user, presence, photo);

                    await SetColor(_appState.Presence.Availability, _appState.Presence.Activity);
                    await InteractWithLights();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occurred in running worker");
                throw;
            }
        }

        private async Task InteractWithLights()
        {
            bool previousWorkingHours = false;
            while (await loginService.IsUserAuthenticated())
            {

                bool useWorkingHours = await _mediator.Send(new Core.WorkingHoursServices.UseWorkingHoursCommand());
                bool IsInWorkingHours = await _mediator.Send(new Core.WorkingHoursServices.IsInWorkingHoursCommand());

                try
                {
                    await Task.Delay(Convert.ToInt32(_appState.Config.LightSettings.PollingInterval * 1000));

                    bool touchLight = false;
                    string newColor = "";

                    if (_appState.Config.LightSettings.SyncLights)
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
                                    switch (_appState.Config.LightSettings.HoursPassedStatus)
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
                                _appState.Presence = await System.Threading.Tasks.Task.Run(() => GetPresence());

                                if (newColor == string.Empty)
                                {
                                    await SetColor(_appState.Presence.Availability, _appState.Presence.Activity);
                                }
                                else
                                {
                                    await SetColor(newColor, newColor);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error Occurred Interacting with Lights");
                }
            }
        }

        private async Task<(User, Presence)> GetUserAndPresence()
        {
            try
            {
                var (profile, presence) = await _mediator.Send(new Core.GraphServices.GetProfileAndPresenceCommand());

                _logger.LogInformation($"User is {profile.DisplayName}");
                return (profile, presence);
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
                var photoStream = await _mediator.Send(new Core.GraphServices.GetPhotoCommand());
                var memoryStream = new MemoryStream();
                photoStream.CopyTo(memoryStream);

                var photoBytes = memoryStream.ToArray();
                var base64Photo = $"data:image/gif;base64,{Convert.ToBase64String(photoBytes)}";

                return base64Photo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception getting photo");
                return null;
            }
        }

        public async Task<Presence> GetPresence()
        {
            try
            {
                return await _mediator.Send(new Core.GraphServices.GetPresenceCommand());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred Getting Presence");
                throw;
            }
        }

        private async Task SetColor(string color, string activity = null)
        {
            try
            {
                if (_appState.Config.LightSettings.Hue.IsEnabled)
                {
                    if (!string.IsNullOrEmpty(_appState.Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(_appState.Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(_appState.Config.LightSettings.Hue.SelectedItemId))
                    {
                        if (_appState.Config.LightSettings.Hue.UseRemoteApi)
                        {
                            if (!string.IsNullOrEmpty(_appState.Config.LightSettings.Hue.RemoteBridgeId))
                            {
                                await _mediator.Send(new Core.RemoteHueServices.SetColorCommand
                                {
                                    Availability = color,
                                    LightId = _appState.Config.LightSettings.Hue.SelectedItemId,
                                    BridgeId = _appState.Config.LightSettings.Hue.RemoteBridgeId
                                });
                            }
                        }
                        else
                        {
                            await _mediator.Send(new Core.HueServices.SetColorCommand() { Activity = activity, Availability = color, LightID = _appState.Config.LightSettings.Hue.SelectedItemId });

                        }
                    }
                }

                if (_appState.Config.LightSettings.LIFX.IsEnabled && !string.IsNullOrEmpty(_appState.Config.LightSettings.LIFX.LIFXApiKey))
                {
                    await _mediator.Send(new Core.LifxServices.SetColorCommand() { Availability = color, Activity = activity, LightId = _appState.Config.LightSettings.LIFX.SelectedItemId });
                }

                if (_appState.Config.LightSettings.Yeelight.IsEnabled && !string.IsNullOrEmpty(_appState.Config.LightSettings.Yeelight.SelectedItemId))
                {
                    await _mediator.Send(new PresenceLight.Core.YeelightServices.SetColorCommand { Activity = activity, Availability = color, LightId = _appState.Config.LightSettings.Yeelight.SelectedItemId });
                }

                if (_appState.Config.LightSettings.CustomApi.IsEnabled)
                {
                    string response = await _mediator.Send(new Core.CustomApiServices.SetColorCommand
                    {
                        Activity = activity,
                        Availability = color
                    });
                }

                if (_appState.Config.LightSettings.LocalSerialHost.IsEnabled && !string.IsNullOrEmpty(_appState.Config.LightSettings.LocalSerialHost.SelectedItemId))
                {
                    string response = await _mediator.Send(new Core.LocalSerialHostServices.SetColorCommand
                    {
                        Activity = activity,
                        Availability = color
                    });
                }

                if (_appState.Config.LightSettings.Wiz.IsEnabled)
                {
                    //await _mediator.Send(new Core.WizServices.SetColorCommand
                    //{
                    //    Activity = activity,
                    //    Availability = color,
                    //    LightID = _appState.Config.LightSettings.Wiz.SelectedItemId
                    //});
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception setting color");
                throw;
            }
        }
    }
}

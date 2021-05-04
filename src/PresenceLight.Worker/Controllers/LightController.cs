using System;
using System.Threading.Tasks;

using LifxCloud.NET.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PresenceLight.Core;

namespace PresenceLight.Worker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LightController : ControllerBase
    {
        private readonly BaseConfig Config;
        private MediatR.IMediator _mediator;
        UserAuthService _userAuthService;
        private readonly AppState _appState;
        private ILogger _logger;
        public LightController(
                      IOptionsMonitor<BaseConfig> optionsAccessor,
                      AppState appState, IServiceScopeFactory _scopeFactory,
                      UserAuthService userAuthService,
                      ILogger<LightController> logger)
        {
            Config = optionsAccessor.CurrentValue;

            var _scope = _scopeFactory.CreateScope();
            var ServiceProvider = _scope.ServiceProvider;
            _mediator = ServiceProvider.GetService<MediatR.IMediator>();
            _userAuthService = userAuthService;
            _appState = appState;
            _logger = logger;
        }



        [AllowAnonymous]
        [HttpGet]
        [Route("GetPresence")]
        public async Task<string> GetPresence()
        {
            if (await _userAuthService.IsUserAuthenticated())
            {
                return _appState.Presence.Availability;
            }
            else
            {
                return string.Empty;
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async void UpdateLight(string command)
        {
            if (await _userAuthService.IsUserAuthenticated())
            {
                string availability = "";
                string activity = "";
                using (Serilog.Context.LogContext.PushProperty("Command", command))
                {
                    if (command == "Teams")
                    {
                        _logger.LogDebug("Set Light Mode: Graph");
                        _appState.SetLightMode("Graph");
                        availability = _appState.Presence.Availability;
                        activity = _appState.Presence.Activity;
                    }
                    else
                    {
                        _logger.LogDebug("Set Light Mode: Custom");
                        _logger.LogDebug("Set Custom Color: Offline");
                        _appState.SetLightMode("Custom");
                        _appState.SetCustomColor("Offline");
                        availability = _appState.CustomColor;
                        activity = _appState.CustomColor;
                    }
                }


                if (Config.LightSettings.Hue.IsEnabled && !Config.LightSettings.Hue.UseRemoteApi && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedItemId))
                {
                    await _mediator.Send(new Core.HueServices.SetColorCommand()
                    {
                        Availability = availability,
                        Activity = activity,
                        LightID = Config.LightSettings.Hue.SelectedItemId
                    }).ConfigureAwait(true);
                }

                if (Config.LightSettings.Hue.IsEnabled && Config.LightSettings.Hue.UseRemoteApi && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedItemId))
                {
                    await _mediator.Send(new Core.RemoteHueServices.SetColorCommand
                    {
                        Availability = availability,
                        LightId = Config.LightSettings.Hue.SelectedItemId,
                        BridgeId = Config.LightSettings.Hue.RemoteBridgeId
                    }).ConfigureAwait(true);
                }

                if (Config.LightSettings.LIFX.IsEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey) && !string.IsNullOrWhiteSpace(Config.LightSettings.LIFX.SelectedItemId))
                {
                    await _mediator.Send(new Core.LifxServices.SetColorCommand()
                    {
                        Availability = availability,
                        Activity = activity,
                        LightId = Config.LightSettings.LIFX.SelectedItemId
                    }).ConfigureAwait(true);
                }

                if (Config.LightSettings.Wiz.IsEnabled && !string.IsNullOrWhiteSpace(Config.LightSettings.Wiz.SelectedItemId))
                {
                    await _mediator.Send(new Core.WizServices.SetColorCommand()
                    {
                        Availability = availability,
                        Activity = activity,
                        LightID = Config.LightSettings.Wiz.SelectedItemId
                    }).ConfigureAwait(true);
                }

                if (Config.LightSettings.Yeelight.IsEnabled && !string.IsNullOrWhiteSpace(Config.LightSettings.Yeelight.SelectedItemId))
                {
                    await _mediator.Send(new Core.YeelightServices.SetColorCommand()
                    {
                        Availability = availability,
                        Activity = activity,
                        LightId = Config.LightSettings.Yeelight.SelectedItemId
                    }).ConfigureAwait(true);
                }
            }
        }
    }
}

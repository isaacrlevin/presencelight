using System;

using LifxCloud.NET.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
   
        private readonly AppState _appState;
        private ILogger _logger;
        public LightController(MediatR.IMediator mediator,
                      IOptionsMonitor<BaseConfig> optionsAccessor,
                      AppState appState,
                      ILogger<LightController> logger )
        {
            Config = optionsAccessor.CurrentValue;
            _mediator = mediator;
         
            _appState = appState;
            _logger = logger;
        }



        [AllowAnonymous]
        [HttpGet]
        [Route("GetPresence")]
        public string GetPresence()
        {
            return _appState.Presence.Availability;
        }

        [AllowAnonymous]
        [HttpGet]
        public async void UpdateLight(string command)
        {
            using (Serilog.Context.LogContext.PushProperty("Command", command))
            {
                if (command == "Teams")
                {
                    _logger.LogDebug("Set Light Mode: Graph");
                    _appState.SetLightMode("Graph");
                }
                else
                {
                    _logger.LogDebug("Set Light Mode: Custom");
                    _logger.LogDebug("Set Custom Color: Offline");
                    _appState.SetLightMode("Custom");
                    _appState.SetCustomColor("Offline");
                }
            }

            if (_appState.LightMode == "Custom")
            {
                if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedItemId))
                {
                    await _mediator.Send(new Core.HueServices.SetColorCommand()
                    {
                        Availability = _appState.CustomColor,
                        Activity = "",
                        LightID = Config.LightSettings.Hue.SelectedItemId
                    });
                    
                }

                if (Config.LightSettings.LIFX.IsEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey))
                {
                
                    await _mediator.Send(new Core.LifxServices.SetColorCommand() { Availability = _appState.CustomColor, Activity = "", LightId = Config.LightSettings.LIFX.SelectedItemId });

                }
            }
        }

    }
}

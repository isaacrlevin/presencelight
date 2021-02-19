﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;

using LifxCloud.NET.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using PresenceLight.Core;

namespace PresenceLight.Worker.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AlexaController : ControllerBase
    {
        private readonly BaseConfig Config;
  
        private ILogger _logger;
        private MediatR.IMediator _mediator;
        private readonly AppState _appState;

        public AlexaController(IOptionsMonitor<BaseConfig> optionsAccessor,
                      ILogger<AlexaController> logger,
                      AppState appState,
                      MediatR.IMediator mediator )
        {
            Config = optionsAccessor.CurrentValue;
           
            _appState = appState;
            _mediator = mediator;
            _logger = logger;
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> ProcessAlexaRequest([FromBody] SkillRequest request)
        {
            using (Serilog.Context.LogContext.PushProperty("Request", JsonConvert.SerializeObject(request)))
            {
                var requestType = request.GetRequestType();

                _logger.LogDebug($"Beginning Alexa Request: {requestType.Name}");

                SkillResponse response = null;

                if (requestType == typeof(LaunchRequest))
                {
                    response = ResponseBuilder.Tell("Welcome to Presence Light!");
                    response.Response.ShouldEndSession = false;
                }
                else if (requestType == typeof(IntentRequest))
                {
                    var intentRequest = request.Request as IntentRequest;

                    if (intentRequest.Intent.Name == "Teams")
                    {
                        _appState.SetLightMode("Graph");
                        response = ResponseBuilder.Tell("Presence Light set to Teams!");
                    }
                    else if (intentRequest.Intent.Name == "Custom")
                    {
                        _appState.SetLightMode("Custom");
                        _appState.SetCustomColor("#FFFFFF");
                        response = ResponseBuilder.Tell("Presence Light set to custom!");
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

                return new OkObjectResult(response);
            }
        }

    }
}

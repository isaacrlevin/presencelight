using System;
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
using Microsoft.Extensions.DependencyInjection;
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
                      AppState appState, IServiceScopeFactory _scopeFactory )
        {
            Config = optionsAccessor.CurrentValue;
           
            _appState = appState;
            var _scope = _scopeFactory.CreateScope();
            var ServiceProvider = _scope.ServiceProvider;
            _mediator = ServiceProvider.GetService<MediatR.IMediator>();
            _logger = logger;
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> ProcessAlexaRequest([FromBody] SkillRequest request)
        {
            string availability = "";
            string activity = "";

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

                return new OkObjectResult(response);
            }
        }

    }
}

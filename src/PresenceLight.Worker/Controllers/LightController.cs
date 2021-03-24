using System.Threading.Tasks;

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

                await _mediator.Publish(new SetColorNotification(availability, activity)).ConfigureAwait(true);
            }
        }
    }
}

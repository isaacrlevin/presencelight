using System;
using LifxCloud.NET.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PresenceLight.Core;

namespace PresenceLight.Worker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LightController : ControllerBase
    {
        private readonly BaseConfig Config;
        private readonly IHueService _hueService;
        private LIFXService _lifxService;
        private readonly AppState _appState;
        public LightController(IHueService hueService,
                      IOptionsMonitor<BaseConfig> optionsAccessor,
                      AppState appState,
                      LIFXService lifxService)
        {
            Config = optionsAccessor.CurrentValue;
            _hueService = hueService;
            _lifxService = lifxService;
            _appState = appState;
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
            if (command == "Teams")
            {
                _appState.SetLightMode("Graph");
            }
            else
            {
                _appState.SetLightMode("Custom");
                _appState.SetCustomColor("Offline");
            }

            if (_appState.LightMode == "Custom")
            {
                if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedHueLightId))
                {
                    await _hueService.SetColor(_appState.CustomColor, Config.LightSettings.Hue.SelectedHueLightId);
                }

                if (Config.LightSettings.LIFX.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey))
                {
                    await _lifxService.SetColor(_appState.CustomColor, Config.LightSettings.LIFX.SelectedLIFXItemId);
                }
            }
        }
    }
}

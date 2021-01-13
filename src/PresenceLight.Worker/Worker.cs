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
        private readonly AppState _appState;
        private readonly ILogger<Worker> _logger;
        private LIFXService _lifxService;
        private ICustomApiService _customApiService;

        public Worker(IHueService hueService,
                      ILogger<Worker> logger,
                      IOptionsMonitor<BaseConfig> optionsAccessor,
                      AppState appState,
                      LIFXService lifxService,
                      ICustomApiService customApiService)
        {
            Config = optionsAccessor.CurrentValue;
            _hueService = hueService;
            _lifxService = lifxService;
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
                    try
                    {
                        await GetData();
                    }
                    catch (Exception e)
                    {
                        var foo = e;
                    }
                }
                await Task.Delay(1000, stoppingToken);
            }
        }


        private async Task GetData()
        {
            var user = await GetUserInformation();

            var photo = await GetPhotoAsBase64Async();

            var presence = await GetPresence();

            _appState.SetUserInfo(user, photo, presence);

            if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedHueLightId))
            {
                await _hueService.SetColor(presence.Availability, Config.LightSettings.Hue.SelectedHueLightId);
            }

            if (Config.LightSettings.LIFX.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey))
            {
                await _lifxService.SetColor(presence.Availability, (Selector)Config.LightSettings.LIFX.SelectedLIFXItemId);
            }

            while (_appState.IsUserAuthenticated)
            {
                if (_appState.LightMode == "Graph")
                {
                    presence = await GetPresence();

                    _appState.SetPresence(presence);
                    _logger.LogInformation($"Presence is {presence.Availability}");
                    if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedHueLightId))
                    {
                        await _hueService.SetColor(presence.Availability, Config.LightSettings.Hue.SelectedHueLightId);
                    }

                    if (Config.LightSettings.LIFX.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey))
                    {
                        await _lifxService.SetColor(presence.Availability, (Selector)Config.LightSettings.LIFX.SelectedLIFXItemId);
                    }
                    if (Config.LightSettings.Custom.IsCustomApiEnabled)
                    {
                        // passing the data on only when it changed is handled within the custom api service
                        await _customApiService.SetColor(presence.Availability, presence.Activity);
                    }
                }
                Thread.Sleep(Convert.ToInt32(Config.LightSettings.PollingInterval * 1000));
            }

            _logger.LogInformation("User logged out, no longer polling for presence.");
        }

        public async Task<User> GetUserInformation()
        {
            try
            {
                var me = await _appState.GraphServiceClient.Me.Request().GetAsync();
                _logger.LogInformation($"User is {me.DisplayName}");
                return me;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception getting me: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GetPhotoAsBase64Async()
        {
            try
            {
                var photoStream = await _appState.GraphServiceClient.Me.Photo.Content.Request().GetAsync();
                var memoryStream = new MemoryStream();
                photoStream.CopyTo(memoryStream);

                var photoBytes = memoryStream.ToArray();
                var base64Photo = $"data:image/gif;base64,{Convert.ToBase64String(photoBytes)}";

                return base64Photo;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception getting photo: {ex.Message}");
            }

            return null;
        }

        public async Task<Presence> GetPresence()
        {
            try
            {
                var presence = await _appState.GraphServiceClient.Me.Presence.Request().GetAsync();

                var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

                presence.Activity = r.Replace(presence.Activity, " ");


                _logger.LogInformation($"Presence is {presence.Availability}, Activity is {presence.Activity}.");
                return presence;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception getting presence: {ex.Message}");
                throw;
            }
        }
    }
}

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
                      ICustomApiService customApiService)
        {
            Config = optionsAccessor.CurrentValue;
            _workingHoursService = workingHoursService;
            _hueService = hueService;
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
                        await GetData();
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


        private async Task GetData()
        {
            try
            {
                var user = await GetUserInformation();

                var photo = await GetPhotoAsBase64Async();

                var presence = await GetPresence();

                _appState.SetUserInfo(user, photo, presence);

                if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedItemId))
                {
                    await _hueService.SetColor(presence.Availability, presence.Activity, Config.LightSettings.Hue.SelectedItemId);
                }

                if (Config.LightSettings.LIFX.IsEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey))
                {
                    await _lifxService.SetColor(presence.Availability, presence.Activity, Config.LightSettings.LIFX.SelectedItemId);
                    Helpers.AppendLogger(_logger, $"Setting LIFX Light: { Config.LightSettings.Hue.SelectedItemId}, Graph Presence: {presence.Availability}");
                }

                if (Config.LightSettings.CustomApi.IsEnabled)
                {
                    // passing the data on only when it changed is handled within the custom api service
                    await _customApiService.SetColor(presence.Availability, presence.Activity);
                    Helpers.AppendLogger(_logger, $"Setting Custom Api Light: { Config.LightSettings.CustomApi.SelectedItemId}, Graph Presence: {presence.Availability}, {presence.Activity}");
                }
                if (Config.LightSettings.Yeelight.IsEnabled)
                {
                    // passing the data on only when it changed is handled within the custom api service
                    await _yeelightService.SetColor(presence.Availability, presence.Activity, Config.LightSettings.Yeelight.SelectedItemId);
                    Helpers.AppendLogger(_logger, $"Setting Yeelight Light: { Config.LightSettings.CustomApi.SelectedItemId}, Graph Presence: {presence.Availability}, {presence.Activity} ");
                }

                while (_appState.IsUserAuthenticated)
                {
                    if (_appState.LightMode == "Graph")
                    {
                        presence = await GetPresence();

                        _appState.SetPresence(presence);
                        Helpers.AppendLogger(_logger, $"Presence is {presence.Availability}");
                        if (!string.IsNullOrEmpty(Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(Config.LightSettings.Hue.SelectedItemId))
                        {
                            await _hueService.SetColor(presence.Availability, presence.Activity, Config.LightSettings.Hue.SelectedItemId);
                            Helpers.AppendLogger(_logger, $"Setting Hue Light: { Config.LightSettings.LIFX.SelectedItemId}, Graph Presence: {presence.Availability}, {presence.Activity}");
                        }

                        if (Config.LightSettings.LIFX.IsEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey))
                        {
                            await _lifxService.SetColor(presence.Availability, presence.Activity, Config.LightSettings.LIFX.SelectedItemId);
                            Helpers.AppendLogger(_logger, $"Setting LIFX Light: { Config.LightSettings.LIFX.SelectedItemId}, Graph Presence: {presence.Availability}, {presence.Activity}");
                        }
                        if (Config.LightSettings.CustomApi.IsEnabled)
                        {
                            // passing the data on only when it changed is handled within the custom api service
                            await _customApiService.SetColor(presence.Availability, presence.Activity);
                            Helpers.AppendLogger(_logger, $"Setting Custom Api Light: { Config.LightSettings.CustomApi.SelectedItemId}, Graph Presence: {presence.Availability}, {presence.Activity}");
                        }
                        if (Config.LightSettings.Yeelight.IsEnabled)
                        {
                            // passing the data on only when it changed is handled within the custom api service
                            await _yeelightService.SetColor(presence.Availability, presence.Activity, Config.LightSettings.Yeelight.SelectedItemId);
                            Helpers.AppendLogger(_logger, $"Setting Yeelight Light: { Config.LightSettings.CustomApi.SelectedItemId}, Graph Presence: {presence.Availability}, {presence.Activity} ");
                        }
                    }
                    Thread.Sleep(Convert.ToInt32(Config.LightSettings.PollingInterval * 1000));
                }

                Helpers.AppendLogger(_logger, "User logged out, no longer polling for presence.");
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Exception occured in running worker", e);
                throw;
            }
        }

        public async Task<User> GetUserInformation()
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

        public async Task<string> GetPhotoAsBase64Async()
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

        public async Task<Presence> GetPresence()
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
    }
}

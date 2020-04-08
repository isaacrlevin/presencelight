using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Newtonsoft.Json;
using PresenceLight.Core;
using PresenceLight.Core.Graph;
using PresenceLight.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ConfigWrapper Config;
        private readonly IHueService _hueService;
        private readonly AppState _appState;
        private readonly ILogger<Worker> _logger;
        public Worker(IGraphService graphService, IHueService hueService, ILogger<Worker> logger, IOptionsMonitor<ConfigWrapper> optionsAccessor, AppState appState)
        {
            Config = optionsAccessor.CurrentValue;
            _hueService = hueService;
            _logger = logger;
            _appState = appState;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!Debugger.IsAttached)
            {
                OpenBrowser("https://localhost:5001");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                if (!string.IsNullOrEmpty(_appState.AccessToken))
                {
                    try
                    {
                        await GetData(_appState.AccessToken);
                    }
                    catch { }
                    await Task.Delay(5000, stoppingToken);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void OpenBrowser(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    System.Diagnostics.Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    System.Diagnostics.Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    System.Diagnostics.Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task GetData(string token)
        {
            var user = await GetUserInformation(token);

            var photo = await GetPhotoAsBase64Async(token);

            var presence = await GetPresence(token);

            _appState.SetUserInfo(user, photo, presence);

            if (!string.IsNullOrEmpty(Config.HueApiKey) && !string.IsNullOrEmpty(Config.HueIpAddress) && !string.IsNullOrEmpty(Config.SelectedHueLightId))
            {
                await _hueService.SetColor(presence.Availability, Config.SelectedHueLightId);
            }

            while (true)
            {

                presence = await GetPresence(token);

                _appState.SetPresence(presence);
                _logger.LogInformation($"Presence is {presence.Availability}");
                if (!string.IsNullOrEmpty(Config.HueApiKey) && !string.IsNullOrEmpty(Config.HueIpAddress) && !string.IsNullOrEmpty(Config.SelectedHueLightId))
                {
                    await _hueService.SetColor(presence.Availability, Config.SelectedHueLightId);
                }

                Thread.Sleep(5000);
            }
        }

        public async Task<User> GetUserInformation(string accessToken)
        {
            HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer",
            accessToken);
            var response = await httpClient.GetAsync($"https://graph.microsoft.com//beta/me");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var me = JsonConvert.DeserializeObject<User>
                    (content);
                _logger.LogInformation($"User is {me.DisplayName}");
                return me;
            }

            throw new
            HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }

        public async Task<string> GetPhotoAsBase64Async(string accessToken)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer",
            accessToken);

            var response = await httpClient.GetAsync($"https://graph.microsoft.com/beta/me/photo/$value");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                byte[] photo = await response.Content.ReadAsByteArrayAsync();
                var base64 = Convert.ToBase64String(photo);
                var photoBase64 = String.Format("data:image/gif;base64,{0}", base64);

                return photoBase64;
            }
            else
            {
                return null;
            }
        }

        public async Task<Presence> GetPresence(string accessToken)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer",
            accessToken);

            var response = await httpClient.GetAsync($"https://graph.microsoft.com//beta/me/presence");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var presence = JsonConvert.DeserializeObject<Presence>
                    (content);
                _logger.LogInformation($"Presence is {presence.Availability}");
                return presence;
            }

            throw new
            HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
        }

    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Newtonsoft.Json;
using PresenceLight.Core;
using PresenceLight.Core.Graph;
using PresenceLight.Core.Helpers;
using Q42.HueApi;

namespace PresenceLight.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ConfigWrapper _options;
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IGraphService _graphservice;
        private bool stopPolling;
        private readonly IHueService _hueService;
        private readonly AppState _appState;
        private readonly ILogger<Worker> _logger;
        public Worker(IGraphService graphService, IHueService hueService, ILogger<Worker> logger, IOptionsMonitor<ConfigWrapper> optionsAccessor, AppState appState)
        {
            _options = optionsAccessor.CurrentValue;
            _graphservice = graphService;
            _hueService = hueService;
            _logger = logger;
            _appState = appState;
            _graphServiceClient = _graphservice.GetAuthenticatedGraphClient(typeof(DeviceCodeFlowAuthorizationProvider));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!Debugger.IsAttached)
            {
                OpenBrowser("https://localhost:5001");
            }
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var (profile, presence) = await GetBatchContent();
                    var photo = await GetPhoto();

                    _appState.SetUserInfo(profile, photo, presence);

                    if (!string.IsNullOrEmpty(_options.HueApiKey) && !string.IsNullOrEmpty(_options.HueIpAddress))
                    {
                        await _hueService.SetColor(presence.Availability, _appState.LightId);
                    }

                    while (true)
                    {
                        if (stopPolling)
                        {
                            stopPolling = false;
                            return;
                        }
                        await Task.Delay(5000);
                        try
                        {
                            presence = await GetPresence();

                            _appState.SetPresence(presence);

                            if (!string.IsNullOrEmpty(_options.HueApiKey) && !string.IsNullOrEmpty(_options.HueIpAddress))
                            {
                                await _hueService.SetColor(presence.Availability, _appState.LightId);
                            }
                        }
                        catch { }
                    }

                }
                catch (Exception e)
                {
                    _logger.LogError($"{e.Message}");
                }

                await Task.Delay(5000, stoppingToken);
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


        private async Task<Presence> GetPresence()
        {
            if (!stopPolling)
            {
                return await _graphServiceClient.Me.Presence.Request().GetAsync();
            }
            else
            {
                throw new Exception();
            }
        }

        private async Task<byte[]> GetPhoto()
        {
            return ReadFully(await _graphServiceClient.Me.Photo.Content.Request().GetAsync());
        }

        private static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        private async Task<(User User, Presence Presence)> GetBatchContent()
        {
            IUserRequest userRequest = _graphServiceClient.Me.Request();
            IPresenceRequest presenceRequest = _graphServiceClient.Me.Presence.Request();

            BatchRequestContent batchRequestContent = new BatchRequestContent();

            var userRequestId = batchRequestContent.AddBatchRequestStep(userRequest);
            var presenceRequestId = batchRequestContent.AddBatchRequestStep(presenceRequest);

            BatchResponseContent returnedResponse = await _graphServiceClient.Batch.Request().PostAsync(batchRequestContent);

            User user = await returnedResponse.GetResponseByIdAsync<User>(userRequestId);
            Presence presence = await returnedResponse.GetResponseByIdAsync<Presence>(presenceRequestId);

            return (User: user, Presence: presence);
        }
    }
}

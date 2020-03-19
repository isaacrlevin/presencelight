using System;
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

namespace PresenceLight.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ConfigWrapper _options;
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IGraphService _graphservice;
        private readonly IHueService _hueService;
        private readonly ILogger<Worker> _logger;
        public Worker(IGraphService graphService, IHueService hueService, ILogger<Worker> logger, IOptionsMonitor<ConfigWrapper> optionsAccessor)
        {
            _options = optionsAccessor.CurrentValue;
            _graphservice = graphService;
            _hueService = hueService;
            _logger = logger;
            _graphServiceClient = _graphservice.GetAuthenticatedGraphClient(typeof(DeviceCodeFlowAuthorizationProvider));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (string.IsNullOrEmpty(_options.HueApiKey))
                    {
                       await _hueService.RegisterBridge();
                        System.IO.File.WriteAllText($"{System.IO.Directory.GetCurrentDirectory()}/appsettings.json", JsonConvert.SerializeObject(_options));
                    }

                    var graphResult = _graphServiceClient.Me.Presence.Request().GetAsync().Result;
                    await _hueService.SetColor(graphResult.Availability, "1");
                }
                catch (Exception e)
                {
                    _logger.LogError($"{e.Message}");
                }

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using PresenceLight.Core;
using PresenceLight.Core.Graph;
using PresenceLight.Core.Helpers;

namespace PresenceLight.Worker
{
    public class Worker : BackgroundService
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IGraphService _graphservice;
        private readonly IHueService _hueService;
        private readonly ILogger<Worker> _logger;
        private readonly ConfigWrapper _options;
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
                    var graphResult = _graphServiceClient.Me.Presence.Request().GetAsync().Result;
                    await _hueService.SetColor(graphResult.Availability);
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

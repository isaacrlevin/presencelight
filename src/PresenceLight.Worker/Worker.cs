using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using PresenceLight.Core;
using PresenceLight.Core.Graph;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Original;
using Q42.HueApi.Interfaces;

namespace PresenceLight.Worker
{
    public class Worker : BackgroundService
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IGraphService _graphservice;
        private readonly IHueService _hueService;
        public Worker(IGraphService graphService, IHueService hueService)
        {
            _graphservice = graphService;
            _hueService = hueService;
            _graphServiceClient = _graphservice.GetAuthenticatedGraphClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var graphResult = _graphServiceClient.Me.Presence.Request().GetAsync().Result;
                await _hueService.SetColor(graphResult.Availability);
                Thread.Sleep(5000);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}

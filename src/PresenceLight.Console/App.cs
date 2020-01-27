using Microsoft.Extensions.Options;
using Microsoft.Graph;
using PresenceLight.Core;
using PresenceLight.Core.Graph;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Original;
using Q42.HueApi.Interfaces;
using System;
using System.Threading;

namespace PresenceLight.Console
{
    public class App
    {
        private readonly ConfigWrapper _options;
        private readonly IGraphService _graphservice;
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IHueService _hueService;

        public App(IOptionsMonitor<ConfigWrapper> optionsAccessor, IGraphService graphService, IHueService hueService)
        {
            _options = optionsAccessor.CurrentValue;
            _graphservice = graphService;
            _hueService = hueService;
            _graphServiceClient = _graphservice.GetAuthenticatedGraphClient();
        }

        public ConfigWrapper Options => _options;

        public async void Run()
        {
            while (true)
            {
                var graphResult = _graphServiceClient.Me.Presence.Request().GetAsync().Result;
                await _hueService.SetColor(graphResult.Availability);
                Thread.Sleep(5000);
            }
        }
    }
} 
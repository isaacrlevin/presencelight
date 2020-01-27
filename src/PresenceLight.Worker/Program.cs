using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PresenceLight.Core;
using PresenceLight.Core.Graph;
using Q42.HueApi;
using Q42.HueApi.Interfaces;

namespace PresenceLight.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseWindowsService()
             .ConfigureLogging(logging =>
             {
                 logging.AddEventLog();
             })
             .ConfigureServices((hostContext, services) =>
             {
                 services.AddOptions();
                 services.Configure<ConfigWrapper>(hostContext.Configuration);
                 services.AddSingleton<IGraphService, GraphService>();
                 services.AddSingleton<IHueService, HueService>();
                 services.AddHostedService<Worker>();
             });
    }
}

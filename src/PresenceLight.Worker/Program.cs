using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PresenceLight.Core;
using PresenceLight.Core.Graph;

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
             .ConfigureAppConfiguration((hostContext, config) => {
                 // Configure the app here.
                 config
                     .SetBasePath(Environment.CurrentDirectory)
                     .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                     .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);

                 config.AddEnvironmentVariables();
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

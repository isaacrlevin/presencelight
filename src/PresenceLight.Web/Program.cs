using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using PresenceLight.Core;

using Serilog;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PresenceLight.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            IConfigurationBuilder configBuilderForMain = new ConfigurationBuilder();
            ConfigureConfiguration(configBuilderForMain);

            IConfiguration configForMain = configBuilderForMain.Build();

            var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.InstrumentationKey = configForMain["ApplicationInsights:InstrumentationKey"];

            Log.Logger = new LoggerConfiguration()
                 .ReadFrom.Configuration(configForMain)
                 .WriteTo.PresenceEventsLogSink()
                 .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces, Serilog.Events.LogEventLevel.Error)
                 .Enrich.FromLogContext()
                 .CreateLogger();


            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

            var builder = Host.CreateDefaultBuilder(args);

            builder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.Sources.Clear();

                    var env = hostingContext.HostingEnvironment;

                    ConfigureConfiguration(config);

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                  .ConfigureLogging(setup => {
                      setup.AddSerilog(Log.Logger);
                  })
                .UseStartup<Startup>();
            }).UseSerilog();

            return builder;
        }

        private static void ConfigureConfiguration(IConfigurationBuilder config)
        {
            config
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddJsonFile("PresenceLightSettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile("PresenceLightSettings.Development.json", optional: true, reloadOnChange: false)
                .AddJsonFile(System.IO.Path.Combine("config", "appsettings.json"), optional: true, reloadOnChange: false)
                .AddJsonFile(System.IO.Path.Combine("config", "PresenceLightSettings.json"), optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            config.Build();
        }
    }
}

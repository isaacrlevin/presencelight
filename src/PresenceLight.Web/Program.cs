using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

            Host.CreateDefaultBuilder(args)
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
                .UseStartup<Startup>();
            });

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

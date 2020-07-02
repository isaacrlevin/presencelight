using System;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.IO;

namespace PresenceLight.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static void ConfigureConfiguration(IConfigurationBuilder config)
        {
            config
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("PresenceLightSettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddUserSecrets<Startup>();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            IConfigurationBuilder configBuilderForMain = new ConfigurationBuilder();
            ConfigureConfiguration(configBuilderForMain);
            IConfiguration configForMain = configBuilderForMain.Build();

            return Host.CreateDefaultBuilder(args)
                   .ConfigureAppConfiguration(ConfigureConfiguration)
                   .ConfigureWebHostDefaults(webBuilder =>
                   {
                       if (Convert.ToBoolean(configForMain["DeployedToServer"]))
                       {
                           webBuilder
                           .ConfigureKestrel(options =>
                           {

                               var server = Dns.GetHostName();
                               IPHostEntry heserver = Dns.GetHostEntry(server);
                               var ip = heserver.AddressList.Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault();

                               if (ip != null)
                               {
                                   options.Listen(ip, 5001);
                                   options.Listen(ip, 5002, listenOptions =>
                                   {
                                       listenOptions.UseHttps();
                                       listenOptions.UseHttps("presencelight.pfx", "presencelight");
                                   });

                               }
                           });
                       }
                       webBuilder.UseStartup<Startup>();
                   });
        }
    }
}

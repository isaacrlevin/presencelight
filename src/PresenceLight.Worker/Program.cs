using System;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.IO;
using System.Security.Authentication;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;

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
                .AddJsonFile("PresenceLightSettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddUserSecrets<Startup>();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            IConfigurationBuilder configBuilderForMain = new ConfigurationBuilder();
            ConfigureConfiguration(configBuilderForMain);
            IConfiguration configForMain = configBuilderForMain.Build();

            Console.WriteLine($"Deployed to server: {configForMain["DeployedToServer"]}");

            Console.WriteLine($"Server IP: {configForMain["ServerIP"]}");

            return Host.CreateDefaultBuilder(args)
                   .UseSystemd()
                   .ConfigureAppConfiguration(ConfigureConfiguration)
                   .ConfigureWebHostDefaults(webBuilder =>
                   {
                       webBuilder
                       .ConfigureKestrel(options =>
                       {
                           if (Convert.ToBoolean(configForMain["DeployedToServer"]))
                           {
                               var server = Dns.GetHostName();
                               IPHostEntry heserver = Dns.GetHostEntry(server);
                               var ip = heserver.AddressList.Where(a => a.ToString() == "192.168.86.20").FirstOrDefault();

                               if (ip == null)
                               {
                                   NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                                   foreach (NetworkInterface adapter in nics)
                                   {
                                       foreach (var x in adapter.GetIPProperties().UnicastAddresses)
                                       {
                                           if (x.Address.ToString() == "192.168.86.20")
                                           {
                                               Console.WriteLine(x.Address.ToString());
                                               ip = x.Address;
                                           }
                                       }
                                   }
                               }

                               if (ip != null)
                               {
                                   Console.WriteLine(ip.ToString());
                                   options.Listen(ip, 5000);
                                   options.Listen(ip, 5001, listenOptions =>
                                   {
                                       listenOptions.UseHttps("presencelight.pfx", "presencelight");
                                   });

                               }
                           }
                           if (Convert.ToBoolean(configForMain["DeployedToContainer"]))
                           {
                               options.ConfigureHttpsDefaults(listenOptions =>
                               {
                                   listenOptions.SslProtocols = SslProtocols.Tls12;
                               });
                           }
                       });
                       webBuilder.UseStartup<Startup>();
                   });
        }
    }
}

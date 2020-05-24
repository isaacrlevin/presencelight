using System;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Linq;
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
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile(
                    "AADSettings.json", optional: false, reloadOnChange: false);
                    config.AddUserSecrets<Startup>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options =>
                    {
                        var server = Dns.GetHostName();
                        IPHostEntry heserver = Dns.GetHostEntry(server);
                        var ip = heserver.AddressList.Where(a => a.ToString().StartsWith("192")).FirstOrDefault();

                        options.Listen(ip, 5001);
                        options.Listen(ip, 5002, listenOptions =>
                        {
                            listenOptions.UseHttps("presencelight.pfx", "presencelight");
                        });

                        // Set properties and call methods on options
                    }).UseStartup<Startup>();
                });
    }
}

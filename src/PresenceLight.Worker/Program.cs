using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using PresenceLight.Core;

using Serilog;


namespace PresenceLight.Worker
{
    public class Program
    {
        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static async Task Main(string[] args)
        {


            try
            {
                var builder = CreateHostBuilder(args).Build();
                Log.Debug("Starting PresenceLight");
                await builder.RunAsync();
            }
            catch (Exception ex)
            {
                //Log: catch setup errors
                Log.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)

                Log.CloseAndFlush();
            }
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


            var builder = Host.CreateDefaultBuilder(args);
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                && Convert.ToBoolean(configForMain["RunSystemD"]))
            {
                builder.UseSystemd();
            }
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
                //.ConfigureKestrel(options =>
                //{
                //    //if (Convert.ToBoolean(configForMain["DeployedToServer"]))
                //    //{
                //    //    if (string.IsNullOrEmpty(configForMain["ServerIP"]) || configForMain["ServerIP"] != "192.168.86.27")
                //    //    {
                //    //        throw new ArgumentException("Supplied Server Ip Address is not configured or it is not in list of redirect Uris for Azure Active Directory");
                //    //    }
                       
                   
                //    //    options.Listen(System.Net.IPAddress.Parse(configForMain["ServerIP"]), 5001, listenOptions =>
                //    //    {
                //    //        var envCertPath = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path");
                //    //        if (string.IsNullOrEmpty(envCertPath))
                //    //        {
                //    //            // Cert Env Not provided, use appsettings
                //    //            //assumes cert is at same level as exe
                //    //            listenOptions.UseHttps(configForMain["Certificate:Name"], configForMain["Certificate:Password"]);
                //    //        }
                //    //        else
                //    //        { }
                //    //    });
                        
                //    //    options.Listen(System.Net.IPAddress.Parse(configForMain["ServerIP"]), 5000, listenOptions =>
                //    //    {
                //    //    });
                //    //}
                //    //else
                //    //{
                        
                //    //    options.ListenLocalhost(5001, listenOptions =>
                //    //    {
                //    //        var envCertPath = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path");
                //    //        if (string.IsNullOrEmpty(envCertPath) && !string.IsNullOrWhiteSpace(configForMain["Certificate:Name"]) && !string.IsNullOrWhiteSpace(configForMain["Certificate:Password"]))
                //    //        {
                //    //        // Cert Env Not provided, use appsettings
                //    //        //assumes cert is at same level as exe
                //    //        listenOptions.UseHttps(configForMain["Certificate:Name"], configForMain["Certificate:Password"]);
                //    //        }
                //    //        else
                //    //        { }
                //    //    });
                        
                //    //    options.ListenLocalhost(5000, listenOptions =>
                //    //    {
                //    //    });
                //    //}

                //    //if (Convert.ToBoolean(configForMain["DeployedToContainer"]))
                //    //{
                //    //    options.ConfigureHttpsDefaults(listenOptions =>
                //    //    {
                //    //        listenOptions.SslProtocols = SslProtocols.Tls12;
                //    //    });
                //    //}
                //})
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging(setup =>
                {
                    setup.AddSerilog(Log.Logger);
                })
                .UseStartup<Startup>();

            })
            .UseSerilog();
            return builder;
        }
    }
}

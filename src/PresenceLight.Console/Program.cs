using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PresenceLight.Core;
using PresenceLight.Core.Graph;

namespace PresenceLight.Console
{

    class Program
    {
        static void Main()
        {
            // create service collection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // create service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // entry to run app
            serviceProvider.GetService<App>().Run();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // build configuration
            var configuration = new ConfigurationBuilder()
              .SetBasePath(System.IO.Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", false)
              .Build();
            serviceCollection.AddOptions();
            serviceCollection.Configure<ConfigWrapper>(configuration.GetSection("AppSettings"));
            serviceCollection.AddSingleton<IGraphService, GraphService>();
            serviceCollection.AddSingleton<IHueService, HueService>();

            // add app
            serviceCollection.AddTransient<App>();
        }
    }
}
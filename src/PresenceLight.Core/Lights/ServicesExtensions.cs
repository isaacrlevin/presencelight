using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PresenceLight.Core.MqttServices;

namespace PresenceLight.Core
{
    public static class ServicesExtensions
    {
        public static void AddPresenceServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IWorkingHoursService, WorkingHoursService>();
            services.AddSingleton<GraphWrapper>();
            services.AddSingleton<IHueService, HueService>();
            services.AddSingleton<IRemoteHueService, RemoteHueService>();
            services.AddSingleton<LIFXService>();
            services.AddSingleton<IYeelightService, YeelightService>();
            services.AddSingleton<ICustomApiService, CustomApiService>();
            services.AddSingleton<IWizService, WizService>();

            services.AddMqttNotificationChannel(config);
        }
    }
}

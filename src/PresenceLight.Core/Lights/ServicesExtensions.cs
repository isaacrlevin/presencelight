
using Microsoft.Extensions.DependencyInjection;

namespace PresenceLight.Core
{
    public static class ServicesExtensions
    {
        public static void AddPresenceServices(this IServiceCollection services)
        {
            services.AddSingleton<IWorkingHoursService, WorkingHoursService>();
            services.AddSingleton<GraphWrapper>();
            services.AddSingleton<IHueService, HueService>();
            services.AddSingleton<IRemoteHueService, RemoteHueService>();
            services.AddSingleton<LIFXService>();
            services.AddSingleton<IYeelightService, YeelightService>();
            services.AddSingleton<ICustomApiService, CustomApiService>();
            services.AddSingleton<IWizService, WizService>();
        }
    }
}

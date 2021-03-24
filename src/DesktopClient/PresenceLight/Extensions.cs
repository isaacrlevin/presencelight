using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using Microsoft.Extensions.DependencyInjection;

using PresenceLight.Core;
using PresenceLight.Core.WizServices;
using PresenceLight.ViewModels;

namespace PresenceLight
{
    public static class Extensions
    {
        public static System.Windows.Media.Color MapColor(this string hexColor)
        {
            return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hexColor);
        }

        public static BitmapImage? LoadImage(this Stream imageData)
        {

            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();

            imageData.Position = 0;
            image.BeginInit();
            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = null;
            image.StreamSource = imageData;
            image.EndInit();

            image.Freeze();
            return image;
        }

        public static void AddViewModels(this IServiceCollection services)
        {
            var allTypes = Assembly.GetExecutingAssembly().GetTypes();
            Type[] viewModels = allTypes.Where(x => !x.IsAbstract && x.IsAssignableTo(typeof(IBaseVm))).ToArray();

            foreach (Type type in viewModels)
            {
                services.AddSingleton(type);
            }

            services.AddSingleton<IEnumerable<IRefreshable>>(x =>
            {
                var refreshables = new List<IRefreshable>();

                foreach (Type type in viewModels)
                {
                    refreshables.Add((IRefreshable)x.GetRequiredService(type));
                }

                return refreshables;
            });
        }

        public static void AddMockLightServices(this IServiceCollection services)
        {
            services.AddSingleton<IWorkingHoursService, WorkingHoursService>();
            services.AddSingleton<GraphWrapper>();
            services.AddSingleton<IHueService, HueService>();
            services.AddSingleton<IRemoteHueService, RemoteHueService>();
            services.AddSingleton<LIFXService>();
            services.AddSingleton<IYeelightService, YeelightService>();
            services.AddSingleton<ICustomApiService, CustomApiService>();
            services.AddSingleton<IWizService, MockWizService>();
        }

        internal class MockWizService : IWizService
        {
            private static readonly IEnumerable<WizLight> Lights = new WizLight []
                {
                    new WizLight { LightName = "Light1", MacAddress = "address1" },
                    new WizLight { LightName = "Light2", MacAddress = "address2" },
                    new WizLight { LightName = "Light3", MacAddress = "address3" }
                };

            public Task<IEnumerable<WizLight>> GetLights() => Task.FromResult(Lights);

            public Task SetColor(string availability, string activity, string lightId)
            {
                return Task.CompletedTask;
            }
        }
    }
}

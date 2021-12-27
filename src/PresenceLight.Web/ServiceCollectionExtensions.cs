using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Blazored.Modal;

using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PresenceLight.Core;
using PresenceLight.Razor;
using PresenceLight.Razor.Services;

namespace PresenceLight.Web
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPresenceLight(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddMediatR(typeof(PresenceLightApp),
                                typeof(BaseConfig));



            services.AddHttpClient();

            services.AddHttpContextAccessor();

            services.Configure<BaseConfig>(Configuration);
            services.AddSingleton<ISettingsService, WebAppSettingsService>();

            services.AddOptions();
            services.AddSingleton<AppState>();
            services.AddPresenceServices();
            services.AddBlazoredModal();

            services.AddBlazorise(options =>
            {
                options.ChangeTextOnKeyPress = true;
            }).AddBootstrapProviders()
.AddFontAwesomeIcons();

            return services;
        }
    }
}

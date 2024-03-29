﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MudBlazor.Services;

using PresenceLight.Core;
using PresenceLight.Razor;
using PresenceLight.Razor.Services;
using PresenceLight.Razor.Components;
namespace PresenceLight.Web
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPresenceLight(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(PresenceLightClientApp).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(BaseConfig).Assembly);
            });

            services.AddMudServices();

            services.AddHttpClient();

            services.AddHttpContextAccessor();

            services.Configure<BaseConfig>(Configuration);
            services.AddSingleton<ISettingsService, WebAppSettingsService>();

            services.AddOptions();
            services.AddSingleton<AppState>();
            services.AddSingleton<AppInfo, AppInfo>();
            services.AddPresenceServices();

            return services;
        }
    }
}

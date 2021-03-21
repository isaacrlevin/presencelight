using System;
using Blazored.Modal;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using PresenceLight.Core;
using System.Threading.Tasks;
using MediatR;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.Extensions.Options;
using Microsoft.ApplicationInsights.SnapshotCollector;
using PresenceLight.Worker.Services;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using IdentityModel;

namespace PresenceLight.Worker
{
    public class Startup
    {
        private class SnapshotCollectorTelemetryProcessorFactory : ITelemetryProcessorFactory
        {
            private readonly IServiceProvider _serviceProvider;

            public SnapshotCollectorTelemetryProcessorFactory(IServiceProvider serviceProvider) =>
                _serviceProvider = serviceProvider;

            public ITelemetryProcessor Create(ITelemetryProcessor next)
            {
                var snapshotConfigurationOptions = _serviceProvider.GetService<IOptions<SnapshotCollectorConfiguration>>();
                return new SnapshotCollectorTelemetryProcessor(next, configuration: snapshotConfigurationOptions.Value);
            }
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var initialScopes = Configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');

            //Need to tell MediatR what Assemblies to look in for Command Event Handlers
            services.AddMediatR(typeof(App),
                                typeof(BaseConfig));

            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
                    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
                        .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
                        .AddInMemoryTokenCaches();

            services.AddControllersWithViews()
                .AddMicrosoftIdentityUI();

            var userAuthService = new UserAuthService(Configuration);
            services.AddSingleton(userAuthService);

            services.AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
                 .Configure<IServiceProvider>((options, serviceProvider) =>
                 {
                     options.ResponseType = OpenIdConnectResponseType.Code;
                     options.UsePkce = false;
                     options.Authority = $"{Configuration["AzureAd:Instance"]}common/v2.0";

                     options.Scope.Add("offline_access");
                     options.Scope.Add("User.Read");

                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         // Azure ID tokens give name in "name"
                         NameClaimType = "name",
                         ValidateIssuer = false
                     };

                     options.Events = new OpenIdConnectEvents
                     {
                         OnAuthenticationFailed = async context =>
                         {
                             context.Response.Redirect("/Error");
                             context.HandleResponse();
                         },

                         OnAuthorizationCodeReceived = async context =>
                     {

                         context.HandleCodeRedemption();

                         var idToken = await userAuthService
                           .AddUserToTokenCache(context.ProtocolMessage.Code);

                         context.HandleCodeRedemption(null, idToken);
                     },
                         OnRedirectToIdentityProviderForSignOut = async context =>
                                              {
                                                  await userAuthService.SignOut();
                                              }
                     };
                 });

            services.AddHttpClient();

            services.AddHttpContextAccessor();

            services.Configure<BaseConfig>(Configuration);
            services.AddSingleton<SettingsService>();

            services.AddOptions();
            services.AddSingleton<AppState>();
            services.AddPresenceServices();
            services.AddBlazoredModal();

            services.AddHostedService<Worker>();

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddRazorPages();

            services.AddServerSideBlazor()
                .AddMicrosoftIdentityConsentHandler();

            services.AddBlazorise(options =>
        {
            options.ChangeTextOnKeyPress = true;
        }).AddBootstrapProviders()
        .AddFontAwesomeIcons();

            services.AddApplicationInsightsTelemetry(options =>
            {
                options.InstrumentationKey = Configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");
                options.EnablePerformanceCounterCollectionModule = false;
                options.EnableDependencyTrackingTelemetryModule = false;
                options.EnableAdaptiveSampling = false;
            });
            services.Configure<SnapshotCollectorConfiguration>(Configuration.GetSection(nameof(SnapshotCollectorConfiguration)));
            services.AddSingleton<ITelemetryProcessorFactory>(sp => new SnapshotCollectorTelemetryProcessorFactory(sp));

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var configuration = app.ApplicationServices.GetService<TelemetryConfiguration>();

            var builder = configuration.DefaultTelemetrySink.TelemetryProcessorChainBuilder;
            double fixedSamplingPercentage = 10;
            builder.UseSampling(fixedSamplingPercentage);

            builder.Build();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}

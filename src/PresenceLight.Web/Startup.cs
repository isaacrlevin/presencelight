using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

using PresenceLight.Razor;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PresenceLight.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var initialScopes = Configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');

            //services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            //    .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"));



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


            services.AddHostedService<Worker>();

            services.AddAuthorization(options =>
            {
                // By default, all incoming requests will be authorized according to the default policy
                options.FallbackPolicy = options.DefaultPolicy;
            });

            services.AddPresenceLight(Configuration);

            services.AddRazorPages();
            services.AddServerSideBlazor()
                .AddMicrosoftIdentityConsentHandler();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

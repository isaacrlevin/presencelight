using System;
using Blazored.Modal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
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
using PresenceLight.Core.Graph;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

namespace PresenceLight.Worker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                    .AddAzureAD(options => Configuration.Bind(options));

            services.AddHttpContextAccessor();
            services.Configure<ConfigWrapper>(Configuration);
            var userAuthService = new UserAuthService(Configuration);
            services.AddSingleton(userAuthService);

            services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
            {
                options.ResponseType = "id_token code";
                options.Authority = $"{Configuration["Instance"]}common/v2.0";
                options.Scope.Add("offline_access");
                options.Scope.Add("User.Read");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Azure ID tokens give name in "name"
                    NameClaimType = "name",
                    ValidateIssuer = false
                };

                // Hook into the OpenID events to wire up MSAL
                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProviderForSignOut = async (context) =>
                    {
                        await userAuthService.SignOut();
                    },
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.Redirect("/Error");
                        context.HandleResponse();
                        return Task.FromResult(0);
                    },
                    OnAuthorizationCodeReceived = async (context) =>
                    {
                        // Prevent ASP.NET Core from handling the code redemption itself
                        context.HandleCodeRedemption();

                        var idToken = await userAuthService
                            .AddUserToTokenCache(context.ProtocolMessage.Code);

                        // Pass the ID token on to the middleware, but
                        // leave access token management to MSAL
                        context.HandleCodeRedemption(null, idToken);
                    }
                };
            });

            services.AddHttpClient();

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddBlazorise(options =>
            {
                options.ChangeTextOnKeyPress = true; // optional
            })
            .AddBootstrapProviders()
            .AddFontAwesomeIcons();

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddOptions();

            services.AddSingleton<IGraphService, GraphService>();
            services.AddSingleton<LIFXService, LIFXService>();
            services.AddSingleton<IHueService, HueService>();
            services.AddSingleton<AppState, AppState>();
            services.AddBlazoredModal();
            services.AddHostedService<Worker>();
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

            app.ApplicationServices
                .UseBootstrapProviders()
                .UseFontAwesomeIcons();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}

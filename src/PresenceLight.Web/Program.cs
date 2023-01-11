using System.Diagnostics;

using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph.ExternalConnectors;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

using MudBlazor;
using MudBlazor.Services;

using PresenceLight.Core;
using PresenceLight.Razor;
using PresenceLight.Web;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

IConfigurationBuilder configBuilderForMain = new ConfigurationBuilder();

configBuilderForMain
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddEnvironmentVariables(); ;

if (Debugger.IsAttached)
{
    configBuilderForMain.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false);
    configBuilderForMain.AddJsonFile("PresenceLightSettings.Development.json", optional: true, reloadOnChange: false);
}
else
{
    configBuilderForMain.AddJsonFile("PresenceLightSettings.json", optional: false, reloadOnChange: false);
    configBuilderForMain.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
}

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    configBuilderForMain.AddJsonFile(System.IO.Path.Combine("config", "appsettings.json"), optional: true, reloadOnChange: false);
    configBuilderForMain.AddJsonFile(System.IO.Path.Combine("config", "PresenceLightSettings.json"), optional: true, reloadOnChange: false);
}


configBuilderForMain.Build();

IConfiguration configForMain = configBuilderForMain.Build();

var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
telemetryConfiguration.InstrumentationKey = configForMain["ApplicationInsights:InstrumentationKey"];

Log.Logger = new LoggerConfiguration()
     .ReadFrom.Configuration(configForMain)
     .WriteTo.PresenceEventsLogSink()
     .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces, Serilog.Events.LogEventLevel.Error)
     .Enrich.FromLogContext()
     .CreateLogger();

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());

if (Debugger.IsAttached)
{
    builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false);
    builder.Configuration.AddJsonFile("PresenceLightSettings.Development.json", optional: true, reloadOnChange: false);
}
else
{
    builder.Configuration.AddJsonFile("PresenceLightSettings.json", optional: false, reloadOnChange: false);
    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
}

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    builder.Configuration.AddJsonFile(System.IO.Path.Combine("config", "appsettings.json"), optional: true, reloadOnChange: false);
    builder.Configuration.AddJsonFile(System.IO.Path.Combine("config", "PresenceLightSettings.json"), optional: true, reloadOnChange: false);
}

builder.Configuration.AddEnvironmentVariables();


builder.Logging.AddSerilog();
builder.Host.UseSerilog();

var initialScopes = builder.Configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
            .AddMicrosoftGraph(builder.Configuration.GetSection("DownstreamApi"))
            .AddInMemoryTokenCaches();


builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

var userAuthService = new UserAuthService(builder.Configuration);
builder.Services.AddSingleton(userAuthService);

builder.Services.AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
 .Configure<IServiceProvider>((options, serviceProvider) =>
 {
     options.ResponseType = OpenIdConnectResponseType.Code;
     options.UsePkce = false;
     options.Authority = $"{builder.Configuration["AzureAd:Instance"]}common/v2.0";

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


builder.Services.AddHostedService<Worker>();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddPresenceLight(builder.Configuration);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddMicrosoftIdentityConsentHandler();

builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
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

app.Run();


using System.Diagnostics;

using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

using PresenceLight.Core;
using PresenceLight.Razor;
using PresenceLight.Razor.Components;
using PresenceLight.Web;

using Serilog;


//var app = ProgramNew.GetWebApplication(args);

var app = ProgramOld.GetWebApplication(args);

app.Run();

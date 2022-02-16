using System.Diagnostics;

using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace PresenceLight.Razor.Services
{
    public class AppVersionTelemetryInitializer : ITelemetryInitializer
    {   
        private readonly AppInfo _appInfo;

        public AppVersionTelemetryInitializer(AppInfo appInfo)
        {
            _appInfo = appInfo;
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Component.Version = _appInfo.GetApplicationVersion();
            telemetry.Context.GlobalProperties["App Version"] = _appInfo.GetApplicationVersion();
            telemetry.Context.GlobalProperties["App Install Type"] = _appInfo.GetAppInstallType();
        }
    }
}

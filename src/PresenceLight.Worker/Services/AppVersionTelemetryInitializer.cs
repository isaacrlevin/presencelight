using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace PresenceLight.Worker.Services
{
    internal class AppVersionTelemetryInitializer : ITelemetryInitializer
    {
        private readonly string _appVersion;

        public AppVersionTelemetryInitializer()
        {
            _appVersion = ThisAppInfo.GetApplicationVersion();
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.GlobalProperties["App Version"] = "Worker";
            telemetry.Context.Component.Version = _appVersion;
        }
    }
}

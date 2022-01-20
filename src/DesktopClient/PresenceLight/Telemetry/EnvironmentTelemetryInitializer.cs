using System;
using System.Diagnostics;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace PresenceLight.Telemetry
{
    internal class EnvironmentTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.GlobalProperties["App Version"] = "Desktop";
            telemetry.Context.GlobalProperties["App Install Type"] = ThisAppInfo.GetAppInstallType();
            telemetry.Context.GlobalProperties["Environment"] = ThisAppInfo.GetPackageChannel() ?? "Local";

            // Always default to Local if we're in the debugger
            if (Debugger.IsAttached)
            {
                telemetry.Context.GlobalProperties["Environment"] = "Local";
            }       
        }
    }
}

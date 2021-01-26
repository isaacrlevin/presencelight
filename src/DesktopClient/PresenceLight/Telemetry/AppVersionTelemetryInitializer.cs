using System;
using System.Reflection;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace PresenceLight.Telemetry
{
    internal class AppVersionTelemetryInitializer : ITelemetryInitializer
    {
        private readonly string _wpfVersion;        
        private readonly string _appVersion;

        public AppVersionTelemetryInitializer()
        {
            _wpfVersion = ThisAppInfo.GetDotNetRuntimeInfo();
            _appVersion = ThisAppInfo.GetPackageVersion();
        }

        public void Initialize(ITelemetry telemetry)
        {            
            telemetry.Context.GlobalProperties[".NET Runtime Version"] = _wpfVersion;            
            telemetry.Context.Component.Version = _appVersion;
        }
    }
}

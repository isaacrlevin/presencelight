using System;
using System.Reflection;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace PresenceLight.Telemetry
{
    internal class AppVersionTelemetryInitializer : ITelemetryInitializer
    {
        private readonly string _runtimeVersion;        
        private readonly string _appVersion;

        public AppVersionTelemetryInitializer()
        {
            _runtimeVersion = ThisAppInfo.GetDotNetRuntimeInfo();
            _appVersion = ThisAppInfo.GetPackageVersion();
        }

        public void Initialize(ITelemetry telemetry)
        {            
            telemetry.Context.GlobalProperties[".NET Runtime Version"] = _runtimeVersion;            
            telemetry.Context.Component.Version = _appVersion;
        }
    }
}

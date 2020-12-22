﻿using System;
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
            _wpfVersion = typeof(System.Windows.Application).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            _appVersion = ThisAppInfo.GetPackageVersion();
        }

        public void Initialize(ITelemetry telemetry)
        {            
            telemetry.Context.GlobalProperties["WPF version"] = _wpfVersion;            
            telemetry.Context.Component.Version = _appVersion;
        }
    }
}

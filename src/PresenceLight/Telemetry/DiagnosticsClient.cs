using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace PresenceLight.Telemetry
{
    public static class DiagnosticsClient
    {
        private static bool _initialized;
        private static TelemetryClient _client;

        public static void Initialize()
        {
            TelemetryConfiguration.Active.TelemetryInitializers.Add(new AppVersionTelemetryInitializer());
            TelemetryConfiguration.Active.TelemetryInitializers.Add(new EnvironmentTelemetryInitializer());

            _initialized = true;
            _client = new TelemetryClient();
           

            System.Windows.Application.Current.Startup += Application_Startup;
            System.Windows.Application.Current.Exit += Application_Exit;
            System.Windows.Application.Current.DispatcherUnhandledException += DispatcherUnhandledException;
        }

        private static void DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            TrackException(e.Exception);
        }

        private static void Application_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            TrackEvent("AppExit");
            _client.Flush();
            // Allow time for flushing:
            System.Threading.Thread.Sleep(1000);
        }

        private static void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
            TrackEvent("AppStart");
        }

        public static void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            if (!_initialized) return;
            _client.TrackEvent(eventName, properties, metrics);
        }

        public static void TrackTrace(string evt)
        {
            if (!_initialized) return;
            _client.TrackTrace(evt);
        }

        public static void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            if (!_initialized) return;
            _client.TrackException(exception, properties, metrics);
        }

        public static void TrackPageView(string pageName)
        {
            if (!_initialized) return;
            _client.TrackPageView(pageName);
        }
    }
}

using System;
using System.Collections.Generic;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace PresenceLight.Telemetry
{
    public class DiagnosticsClient
    {
        private TelemetryClient _client;


        public DiagnosticsClient(TelemetryClient tc)
        {
            _client = tc;

           
            TrackEvent("AppStart");
            System.Windows.Application.Current.Exit += Application_Exit;
            System.Windows.Application.Current.DispatcherUnhandledException += DispatcherUnhandledException;
        }


        private void DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            TrackException(e.Exception);
            e.Handled = true;
        }

        private void Application_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            TrackEvent("AppExit");
            _client.Flush();
            // Allow time for flushing:
            System.Threading.Thread.Sleep(1000);
        }

        private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
            TrackEvent("AppStart");
        }

        public void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            _client.TrackEvent(eventName, properties, metrics);
        }

        public void TrackTrace(string evt)
        {
            _client.TrackTrace(evt);
        }

        public void TrackException(Exception exception, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            _client.TrackException(exception, properties, metrics);
        }

        public void TrackPageView(string pageName)
        {
            _client.TrackPageView(pageName);
        }
    }
}

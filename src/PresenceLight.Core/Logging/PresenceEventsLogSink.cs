using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace PresenceLight.Core 
{
    public class PresenceEventsLogSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;

        public PresenceEventsLogSink(IFormatProvider formatProvider) {
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            PresenceEventsLogHandler?.Invoke(this, logEvent);
        }
      
        public static EventHandler<LogEvent> PresenceEventsLogHandler;
    }
    public class test {
        public test() {
            PresenceEventsLogSink.PresenceEventsLogHandler += Handler;

        }

        private void Handler(object? sender, LogEvent e)
        {
            throw new NotImplementedException();
        }
    }
    public static class PresenceEventsLogSinkExtensions {

        public static LoggerConfiguration PresenceEventsLogSink(
              this LoggerSinkConfiguration loggerConfiguration,
              IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new PresenceEventsLogSink(formatProvider));
        }
    }
}

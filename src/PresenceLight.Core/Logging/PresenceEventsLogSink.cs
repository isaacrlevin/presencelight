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
    /// <summary>
    /// Represents a log sink for presence events.
    /// </summary>
    public class PresenceEventsLogSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PresenceEventsLogSink"/> class with the specified format provider.
        /// </summary>
        /// <param name="formatProvider">The format provider to be used for formatting log messages.</param>
        public PresenceEventsLogSink(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
        }

        /// <summary>
        /// Emits a log event by invoking the PresenceEventsLogHandler delegate.
        /// </summary>
        /// <param name="logEvent">The log event to emit.</param>
        public void Emit(LogEvent logEvent)
        {
            PresenceEventsLogHandler?.Invoke(this, logEvent);
        }

        /// <summary>
        /// Represents the event handler for presence events logging.
        /// </summary>
        public static EventHandler<LogEvent> PresenceEventsLogHandler;
    }


    /// <summary>
    /// Provides extension methods for adding the <see cref="PresenceEventsLogSink"/> to the logger configuration.
    /// </summary>
    public static class PresenceEventsLogSinkExtensions
    {

        /// <summary>
        /// Adds the <see cref="PresenceEventsLogSink"/> to the logger configuration.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>The logger configuration with the added <see cref="PresenceEventsLogSink"/>.</returns>
        public static LoggerConfiguration PresenceEventsLogSink(
              this LoggerSinkConfiguration loggerConfiguration,
              IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new PresenceEventsLogSink(formatProvider));
        }
    }
}

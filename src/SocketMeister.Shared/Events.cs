using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace SocketMeister
{

    /// <summary>
    /// Provides details about a log event.
    /// </summary>
#if SMISPUBLIC
    public partial class LogEventArgs : EventArgs
#else
        internal partial class LogEventArgs : EventArgs
#endif
    {
        private readonly LogEntry _logEntry;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEventArgs"/> class.
        /// </summary>
        /// <param name="logEntry">The log entry containing details about the event.</param>
        public LogEventArgs(LogEntry logEntry)
        {
            _logEntry = logEntry;
        }

        /// <summary>
        /// Gets the log entry containing details about the event.
        /// </summary>
        public LogEntry LogEntry => _logEntry;
    }
}

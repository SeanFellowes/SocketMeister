using System;

namespace SocketMeister
{

    /// <summary>
    /// Log event details.
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        private readonly LogEntry _logEntry;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logEntry">Log entry information</param>
        public LogEventArgs(LogEntry logEntry)
        {
            _logEntry = logEntry;
        }

        /// <summary>
        /// Log entry information
        /// </summary>
        public LogEntry LogEntry => _logEntry;
    }
}

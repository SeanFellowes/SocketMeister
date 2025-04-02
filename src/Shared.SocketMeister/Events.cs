using System;

namespace SocketMeister
{

    /// <summary>
    /// Log event details.
    /// </summary>
#if SMISPUBLIC
    public partial class LogEventArgs : EventArgs
#else
    internal partial class LogEventArgs : EventArgs
#endif
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

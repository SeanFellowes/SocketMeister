using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{
#if !SILVERLIGHT && !SMNOSERVER

    /// <summary>
    /// Status of a socket service
    /// </summary>
    public enum ServiceStatus
    {
        /// <summary>
        /// Service is stopped
        /// </summary>
        Stopped = 0,
        /// <summary>
        /// Service is starting
        /// </summary>
        Starting = 1,
        /// <summary>
        /// Service is started
        /// </summary>
        Started = 2,
        /// <summary>
        /// Service is stopping
        /// </summary>
        Stopping = 3
    }

    /// <summary>
    /// Severity of a trace event
    /// </summary>
    public enum SeverityType
    {
        /// <summary>
        /// Information
        /// </summary>
        Information = 0,
        /// <summary>
        /// Warning
        /// </summary>
        Warning = 1,
        /// <summary>
        /// Error
        /// </summary>
        Error = 2
    }

#endif
}

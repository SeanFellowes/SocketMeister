using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace SocketMeister
{
    /// <summary>
    /// Provides details about an exception that occurred.
    /// </summary>
#if SMISPUBLIC
    public class ExceptionEventArgs : EventArgs
#else
        internal class ExceptionEventArgs : EventArgs
#endif
    {
        private readonly int _eventId;
        private readonly Exception _exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="eventId">The event identifier for this exception. Useful when writing to the Windows Event Log or equivalent.</param>
        public ExceptionEventArgs(Exception exception, int eventId)
        {
            _exception = exception;
            _eventId = eventId;
        }

        /// <summary>
        /// Gets the event identifier for this exception. Useful when writing to the Windows Event Log or equivalent.
        /// </summary>
        public int EventId => _eventId;

        /// <summary>
        /// Gets the exception that occurred.
        /// </summary>
        public Exception Exception => _exception;
    }
}

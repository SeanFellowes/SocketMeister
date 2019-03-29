using System;
using System.IO;
using System.Diagnostics;

namespace SocketMeister
{
    /// <summary>
    /// Raised when an exception occured.
    /// </summary>
    public class ExceptionEventArgs : EventArgs
    {
        private int _eventId;
        private Exception _exception;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Exception">Exception which occured.</param>
        /// <param name="EventId">Event identifier for this exception. Useful if writing this to the Windows Event Log (Or equivalent).</param>
        public ExceptionEventArgs(Exception Exception, int EventId)
        {
            _exception = Exception;
            _eventId = EventId;
        }

        /// <summary>
        /// Event identifier for this exception. Useful if writing this to the Windows Event Log.
        /// </summary>
        public int EventId { get { return _eventId; } }

        /// <summary>
        /// Exception that occured
        /// </summary>
        public Exception Exception { get { return _exception; } }
    }
}

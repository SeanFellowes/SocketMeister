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


    /// <summary>
    /// Exception raised when on the client when the server is stopping
    /// </summary>
    internal class ServerStoppingException : Exception
    {
        /// <summary>
        /// During a SendReceive()  operation, this error may be raised when the socket is closed before a response is returned
        /// </summary>
        internal ServerStoppingException()
            : base("Socket server is stopping") { }

    }

    /// <summary>
    /// During a SendReceive()  operation, this error may be raised when the socket is closed before a response is returned
    /// </summary>
    internal class SocketExceptionSendReceiveTimeout : Exception
    {
        /// <summary>
        /// During a SendReceive()  operation, this error may be raised when the socket is closed before a response is returned
        /// </summary>
        internal SocketExceptionSendReceiveTimeout()
            : base("A response was not received within the timeout period.") { }

        /// <summary>
        /// During a SendReceive()  operation, this error may be raised when the socket is closed before a response is returned
        /// </summary>
        internal SocketExceptionSendReceiveTimeout(string message)
            : base(message) { }

        /// <summary>
        /// During a SendReceive()  operation, this error may be raised when the socket is closed before a response is returned
        /// </summary>
        internal SocketExceptionSendReceiveTimeout(string format, params object[] args)
            : base(string.Format(format, args)) { }

        /// <summary>
        /// During a SendReceive()  operation, this error may be raised when the socket is closed before a response is returned
        /// </summary>
        internal SocketExceptionSendReceiveTimeout(string message, Exception innerException)
            : base(message, innerException) { }

        /// <summary>
        /// During a SendReceive()  operation, this error may be raised when the socket is closed before a response is returned
        /// </summary>
        internal SocketExceptionSendReceiveTimeout(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }
    }
}

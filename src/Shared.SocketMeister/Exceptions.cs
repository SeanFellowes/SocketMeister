using System;

namespace SocketMeister
{
//    /// <summary>
//    /// Raised when an exception occured.
//    /// </summary>
//#if SMISPUBLIC
//    public class ExceptionEventArgs : EventArgs
//#else
//    internal class ExceptionEventArgs : EventArgs
//#endif
//    {
//        private readonly int _eventId;
//        private readonly Exception _exception;

//        /// <summary>
//        /// Constructor
//        /// </summary>
//        /// <param name="Exception">Exception which occured.</param>
//        /// <param name="EventId">Event identifier for this exception. Useful if writing this to the Windows Event Log (Or equivalent).</param>
//        public ExceptionEventArgs(Exception Exception, int EventId)
//        {
//            _exception = Exception;
//            _eventId = EventId;
//        }

//        /// <summary>
//        /// Event identifier for this exception. Useful if writing this to the Windows Event Log.
//        /// </summary>
//        public int EventId { get { return _eventId; } }

//        /// <summary>
//        /// Exception that occured
//        /// </summary>
//        public Exception Exception { get { return _exception; } }
//    }


#if SMISPUBLIC
    public class TraceEventArgs : EventArgs
#else
    internal class TraceEventArgs : EventArgs
#endif
    {
        private readonly int _eventId;
        private readonly string _message;
        private readonly SeverityType _severity;
        private readonly string _source;
        private readonly string _stackTrace;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Message">Message describing the trace event</param>
        /// <param name="Severity">Severity of the trace event.</param>
        /// <param name="EventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
        public TraceEventArgs(string Message, SeverityType Severity, int EventId)
        {
            _message = Message;
            _severity = Severity;
            _eventId = EventId;
            _source = null;
            _stackTrace = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Message">Message describing the trace event</param>
        /// <param name="Severity">Severity of the trace event.</param>
        /// <param name="EventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
        /// <param name="Source">Source of the trace event.</param>
        public TraceEventArgs(string Message, SeverityType Severity, int EventId, string Source)
        {
            _message = Message;
            _severity = Severity;
            _eventId = EventId;
            _source = Source;
            _stackTrace = null; 
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Exception">Exception which occured.</param>
        /// <param name="EventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
        public TraceEventArgs(Exception Exception, int EventId)
        {
            _message = Exception.Message;
            _severity = SeverityType.Error;
            _eventId = EventId;
            _source = null;
            if (Exception.StackTrace != null) _stackTrace = Exception.StackTrace;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Exception">Exception which occured.</param>
        /// <param name="EventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
        /// <param name="Source">Source of the trace event.</param>
        public TraceEventArgs(Exception Exception, int EventId, string Source)
        {
            _message = Exception.Message;
            _severity = SeverityType.Error;
            _eventId = EventId;
            _source = Source;
            if (Exception.StackTrace != null)  _stackTrace = Exception.StackTrace;
        }



        /// <summary>
        /// Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).
        /// </summary>
        public int EventId { get { return _eventId; } }

        /// <summary>
        /// Message describing the trace event
        /// </summary>
        public string Message { get { return _message; } }

        /// <summary>
        /// Severity of the trace event.
        /// </summary>
        public SeverityType Severity {  get { return _severity; } }

        /// <summary>
        /// Optional source of the trace event.
        /// </summary>
        public string Source { get { return _source; } }

        /// <summary>
        /// Optional stack trace information.
        /// </summary>
        public string StackTrace { get { return _stackTrace; } }

    }

}

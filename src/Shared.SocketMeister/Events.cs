using System;

namespace SocketMeister
{

    /// <summary>
    /// Log event details.
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        private readonly LogEventType _eventType;
        private readonly string _message;
        private readonly Severity _severity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message describing the trace event</param>
        /// <param name="severity">Severity of the trace event.</param>
        /// <param name="eventType">Category of event.</param>
        public LogEventArgs(string message, Severity severity, LogEventType eventType)
        {
            _message = message;
            _severity = severity;
            _eventType = eventType;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception">Exception which occured.</param>
        public LogEventArgs(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            _message = exception.ToString();
            _severity = Severity.Error;
            _eventType = LogEventType.Exception;
        }

        /// <summary>
        /// Category of event.
        /// </summary>
        public LogEventType EventType => _eventType;

        /// <summary>
        /// Message describing the trace event
        /// </summary>
        public string Message => _message;

        /// <summary>
        /// Severity of the trace event.
        /// </summary>
        public Severity Severity => _severity;
    }



    /// <summary>
    /// Events and Exceptions raised for analysis and logging purposes
    /// </summary>
    public class TraceEventArgs : EventArgs
    {
        private readonly int _eventId;
        private readonly string _message;
        private readonly Severity _severity;
        private readonly string _source;
        private readonly string _stackTrace;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message describing the trace event</param>
        /// <param name="severity">Severity of the trace event.</param>
        /// <param name="eventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
        public TraceEventArgs(string message, Severity severity, int eventId)
        {
            this._message = message;
            this._severity = severity;
            this._eventId = eventId;
            _source = null;
            _stackTrace = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message describing the trace event</param>
        /// <param name="severity">Severity of the trace event.</param>
        /// <param name="eventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
        /// <param name="source">Source of the trace event.</param>
        public TraceEventArgs(string message, Severity severity, int eventId, string source)
        {
            this._message = message;
            this._severity = severity;
            this._eventId = eventId;
            this._source = source;
            _stackTrace = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception">Exception which occured.</param>
        /// <param name="eventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
        public TraceEventArgs(Exception exception, int eventId)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            _message = exception.Message;
            _severity = Severity.Error;
            this._eventId = eventId;
            _source = null;
            if (exception.StackTrace != null) _stackTrace = exception.StackTrace;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception">Exception which occured.</param>
        /// <param name="eventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
        /// <param name="source">Source of the trace event.</param>
        public TraceEventArgs(Exception exception, int eventId, string source)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            _message = exception.Message;
            _severity = Severity.Error;
            this._eventId = eventId;
            this._source = source;
            if (exception.StackTrace != null) _stackTrace = exception.StackTrace;
        }

        /// <summary>
        /// Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).
        /// </summary>
        public int EventId => _eventId;

        /// <summary>
        /// Message describing the trace event
        /// </summary>
        public string Message => _message;

        /// <summary>
        /// Severity of the trace event.
        /// </summary>
        public Severity Severity => _severity;

        /// <summary>
        /// Optional source of the trace event.
        /// </summary>
        public string Source => _source;

        /// <summary>
        /// Optional stack trace information.
        /// </summary>
        public string StackTrace => _stackTrace;
    }

}

using System;

namespace SocketMeister
{
    /// <summary>
    /// Log entry details, emitted by the logger.
    /// </summary>
#if SMISPUBLIC
    public partial class LogEntry
#else
    internal partial class LogEntry
#endif
    {
        private readonly LogEventType _eventType;
        private readonly Exception _exception;
        private readonly long _messageId;
        private readonly string _message;
        private readonly Severity _severity = Severity.Information;
        private readonly DateTime _timeStamp = DateTime.UtcNow;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception">Exception</param>
        public LogEntry(Exception exception)
            : this(exception, 0) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="messageId">SocketMeister message id this relates to</param>
        public LogEntry(Exception exception, long messageId)
        {
            _eventType = LogEventType.Exception;
            _exception = exception;
            _message = exception.ToString();
            _messageId = messageId;
            _severity = Severity.Error;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="severity">Severity</param>
        /// <param name="eventType">Log event type</param>
        public LogEntry(string message, Severity severity, LogEventType eventType)
        : this(message, severity, eventType, 0) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="severity">Severity</param>
        /// <param name="eventType">Log event type</param>
        /// <param name="messageId">SocketMeister message id this relates to</param>
        public LogEntry(string message, Severity severity, LogEventType eventType, long messageId)
        {
            _eventType = eventType;
            _message = message;
            _messageId = messageId;
            _severity = severity;
        }

        /// <summary>
        /// If the log entry is an exception, this will be the exception object.
        /// </summary>
        public Exception Exception => _exception;

        /// <summary>
        /// Timestamp of the log entry  
        /// </summary>
        public DateTime Timestamp => _timeStamp;

        /// <summary>
        /// Message describing the log event
        /// </summary>
        public string Message => _message;

        /// <summary>
        /// Severity of the log event
        /// </summary>
        public Severity Severity => _severity;

        /// <summary>
        /// Category of the log event
        /// </summary>
        public LogEventType EventType => _eventType;

        /// <summary>
        /// If the log entry is related to a messages, the internal SocketMeister MessageId
        /// </summary>
        public long MessageId => _messageId;
    }
}

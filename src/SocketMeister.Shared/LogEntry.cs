using System;

namespace SocketMeister
{
    /// <summary>
    /// Represents the details of a log entry emitted by the logger.
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
        /// Initializes a new instance of the <see cref="LogEntry"/> class for an exception.
        /// </summary>
        /// <param name="exception">The exception associated with the log entry.</param>
        public LogEntry(Exception exception)
            : this(exception, 0) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntry"/> class for an exception with a specific message ID.
        /// </summary>
        /// <param name="exception">The exception associated with the log entry.</param>
        /// <param name="messageId">The SocketMeister message ID related to this log entry.</param>
        public LogEntry(Exception exception, long messageId)
        {
            _eventType = LogEventType.Exception;
            _exception = exception;
            _message = exception.ToString();
            _messageId = messageId;
            _severity = Severity.Error;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntry"/> class with a message, severity, and event type.
        /// </summary>
        /// <param name="message">The message describing the log event.</param>
        /// <param name="severity">The severity level of the log event.</param>
        /// <param name="eventType">The category of the log event.</param>
        public LogEntry(string message, Severity severity, LogEventType eventType)
            : this(message, severity, eventType, 0) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntry"/> class with a message, severity, event type, and message ID.
        /// </summary>
        /// <param name="message">The message describing the log event.</param>
        /// <param name="severity">The severity level of the log event.</param>
        /// <param name="eventType">The category of the log event.</param>
        /// <param name="messageId">The SocketMeister message ID related to this log entry.</param>
        public LogEntry(string message, Severity severity, LogEventType eventType, long messageId)
        {
            _eventType = eventType;
            _message = message;
            _messageId = messageId;
            _severity = severity;
        }

        /// <summary>
        /// Gets the exception associated with the log entry, if applicable.
        /// </summary>
        public Exception Exception => _exception;

        /// <summary>
        /// Gets the timestamp of the log entry.
        /// </summary>
        public DateTime Timestamp => _timeStamp;

        /// <summary>
        /// Gets the message describing the log event.
        /// </summary>
        public string Message => _message;

        /// <summary>
        /// Gets the severity level of the log event.
        /// </summary>
        public Severity Severity => _severity;

        /// <summary>
        /// Gets the category of the log event.
        /// </summary>
        public LogEventType EventType => _eventType;

        /// <summary>
        /// Gets the SocketMeister message ID related to this log entry, if applicable.
        /// </summary>
        public long MessageId => _messageId;
    }
}

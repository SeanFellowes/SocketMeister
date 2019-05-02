using System;

namespace SocketMeister
{
#if SMISPUBLIC
    public class TraceEventArgs : EventArgs
#else
    internal class TraceEventArgs : EventArgs
#endif
    {
        private readonly int eventId;
        private readonly string message;
        private readonly SeverityType severity;
        private readonly string source;
        private readonly string stackTrace;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message describing the trace event</param>
        /// <param name="severity">Severity of the trace event.</param>
        /// <param name="eventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
        public TraceEventArgs(string message, SeverityType severity, int eventId)
        {
            this.message = message;
            this.severity = severity;
            this.eventId = eventId;
            source = null;
            stackTrace = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message describing the trace event</param>
        /// <param name="severity">Severity of the trace event.</param>
        /// <param name="eventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
        /// <param name="source">Source of the trace event.</param>
        public TraceEventArgs(string message, SeverityType severity, int eventId, string source)
        {
            this.message = message;
            this.severity = severity;
            this.eventId = eventId;
            this.source = source;
            stackTrace = null; 
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception">Exception which occured.</param>
        /// <param name="eventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
        public TraceEventArgs(Exception exception, int eventId)
        {
            message = exception.Message;
            severity = SeverityType.Error;
            this.eventId = eventId;
            source = null;
            if (exception.StackTrace != null) stackTrace = exception.StackTrace;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="exception">Exception which occured.</param>
        /// <param name="eventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
        /// <param name="source">Source of the trace event.</param>
        public TraceEventArgs(Exception exception, int eventId, string source)
        {
            message = exception.Message;
            severity = SeverityType.Error;
            this.eventId = eventId;
            this.source = source;
            if (exception.StackTrace != null)  stackTrace = exception.StackTrace;
        }



        /// <summary>
        /// Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).
        /// </summary>
        public int EventId { get { return eventId; } }

        /// <summary>
        /// Message describing the trace event
        /// </summary>
        public string Message { get { return message; } }

        /// <summary>
        /// Severity of the trace event.
        /// </summary>
        public SeverityType Severity {  get { return severity; } }

        /// <summary>
        /// Optional source of the trace event.
        /// </summary>
        public string Source { get { return source; } }

        /// <summary>
        /// Optional stack trace information.
        /// </summary>
        public string StackTrace { get { return stackTrace; } }

    }

}

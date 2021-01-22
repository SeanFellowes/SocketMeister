


using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{
    /// <summary>
    /// Events and Exceptions raised for analysis and logging purposes
    /// </summary>
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
            if (exception.StackTrace != null) stackTrace = exception.StackTrace;
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
        public SeverityType Severity { get { return severity; } }

        /// <summary>
        /// Optional source of the trace event.
        /// </summary>
        public string Source { get { return source; } }

        /// <summary>
        /// Optional stack trace information.
        /// </summary>
        public string StackTrace { get { return stackTrace; } }
    }


    /// <summary>
    /// Severity of a trace event
    /// </summary>
#if SMISPUBLIC
    public enum SeverityType
#else
    internal enum SeverityType
#endif
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


    /// <summary>
    /// Execution status of a service
    /// </summary>
#if SMISPUBLIC
    public enum SocketServerStatus
#else
    internal enum SocketServerStatus
#endif
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



}


/// <summary>
/// Type of value stored in a token
/// </summary>
#if SMISPUBLIC
public enum ValueType
#else
internal enum ValueType
#endif
{
    /// <summary>
    /// Unknown
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Boolean
    /// </summary>
    BoolValue = 10,
    /// <summary>
    /// DateTime
    /// </summary>
    DateTimeValue = 20,
    /// <summary>
    /// Double Type
    /// </summary>
    DoubleValue = 30,
    /// <summary>
    /// Int16
    /// </summary>
    Int16Value = 40,
    /// <summary>
    /// Int32
    /// </summary>
    Int32Value = 41,
    /// <summary>
    /// Int64
    /// </summary>
    Int64Value = 42,
    /// <summary>
    /// Unsigned Int16
    /// </summary>
    UInt16Value = 50,
    /// <summary>
    /// Unsigned Int32
    /// </summary>
    UInt32Value = 51,
    /// <summary>
    /// Unsigned Int64
    /// </summary>
    UInt64Value = 52,
    /// <summary>
    /// String
    /// </summary>
    StringValue = 60,
    /// <summary>
    /// Byte
    /// </summary>
    ByteValue = 70,
    /// <summary>
    /// Byte Array
    /// </summary>
    ByteArrayValue = 71,
    /// <summary>
    /// Null
    /// </summary>
    NullValue = 99
}




//#if !SILVERLIGHT && !SMNOSERVER
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace SocketMeister
//{
//#if SMISPUBLIC
//    public partial class SocketServer
//#else
//    internal partial class SocketServer
//#endif
//    {

//    }






//#if SMISPUBLIC
//    public class TraceEventArgs : EventArgs
//#else
//    internal class TraceEventArgs : EventArgs
//#endif
//    {
//        private readonly int eventId;
//        private readonly string message;
//        private readonly SeverityType severity;
//        private readonly string source;
//        private readonly string stackTrace;

//        /// <summary>
//        /// Constructor
//        /// </summary>
//        /// <param name="message">Message describing the trace event</param>
//        /// <param name="severity">Severity of the trace event.</param>
//        /// <param name="eventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
//        public TraceEventArgs(string message, SeverityType severity, int eventId)
//        {
//            this.message = message;
//            this.severity = severity;
//            this.eventId = eventId;
//            source = null;
//            stackTrace = null;
//        }

//        /// <summary>
//        /// Constructor
//        /// </summary>
//        /// <param name="message">Message describing the trace event</param>
//        /// <param name="severity">Severity of the trace event.</param>
//        /// <param name="eventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
//        /// <param name="source">Source of the trace event.</param>
//        public TraceEventArgs(string message, SeverityType severity, int eventId, string source)
//        {
//            this.message = message;
//            this.severity = severity;
//            this.eventId = eventId;
//            this.source = source;
//            stackTrace = null;
//        }

//        /// <summary>
//        /// Constructor
//        /// </summary>
//        /// <param name="exception">Exception which occured.</param>
//        /// <param name="eventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
//        public TraceEventArgs(Exception exception, int eventId)
//        {
//            message = exception.Message;
//            severity = SeverityType.Error;
//            this.eventId = eventId;
//            source = null;
//            if (exception.StackTrace != null) stackTrace = exception.StackTrace;
//        }


//        /// <summary>
//        /// Constructor
//        /// </summary>
//        /// <param name="exception">Exception which occured.</param>
//        /// <param name="eventId">Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).</param>
//        /// <param name="source">Source of the trace event.</param>
//        public TraceEventArgs(Exception exception, int eventId, string source)
//        {
//            message = exception.Message;
//            severity = SeverityType.Error;
//            this.eventId = eventId;
//            this.source = source;
//            if (exception.StackTrace != null) stackTrace = exception.StackTrace;
//        }



//        /// <summary>
//        /// Event identifier for this trace event. Useful if writing this to the Windows Event Log (Or equivalent).
//        /// </summary>
//        public int EventId { get { return eventId; } }

//        /// <summary>
//        /// Message describing the trace event
//        /// </summary>
//        public string Message { get { return message; } }

//        /// <summary>
//        /// Severity of the trace event.
//        /// </summary>
//        public SeverityType Severity { get { return severity; } }

//        /// <summary>
//        /// Optional source of the trace event.
//        /// </summary>
//        public string Source { get { return source; } }

//        /// <summary>
//        /// Optional stack trace information.
//        /// </summary>
//        public string StackTrace { get { return stackTrace; } }

//    }



//}
//#endif











//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace SocketMeister
//{
//#if !SILVERLIGHT && !SMNOSERVER

//    /// <summary>
//    /// Status of a socket service
//    /// </summary>
//    public enum ServiceStatus
//    {
//        /// <summary>
//        /// Service is stopped
//        /// </summary>
//        Stopped = 0,
//        /// <summary>
//        /// Service is starting
//        /// </summary>
//        Starting = 1,
//        /// <summary>
//        /// Service is started
//        /// </summary>
//        Started = 2,
//        /// <summary>
//        /// Service is stopping
//        /// </summary>
//        Stopping = 3
//    }


//#endif

//    /// <summary>
//    /// Severity of a trace event
//    /// </summary>
//    public enum SeverityType
//    {
//        /// <summary>
//        /// Information
//        /// </summary>
//        Information = 0,
//        /// <summary>
//        /// Warning
//        /// </summary>
//        Warning = 1,
//        /// <summary>
//        /// Error
//        /// </summary>
//        Error = 2
//    }

//}

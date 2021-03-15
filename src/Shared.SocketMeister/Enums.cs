#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1062 // Validate arguments of public methods


using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{
    /// <summary>
    /// Result of an attempt to process a message. This is included in the MessageResponse.
    /// </summary>
    internal enum MessageEngineDeliveryResult
    {
        /// <summary>
        /// The message was processed successfully
        /// </summary>
        Success = 0,

        /// <summary>
        /// The client or server is shutting down. The message was not processed.
        /// </summary>
        Stopping = 2,

        /// <summary>
        /// There is no process listening for 'MessageReceived' events.
        /// </summary>
        NoMessageReceivedEventListener = 3,

        /// <summary>
        /// An exception occured while processing the message
        /// </summary>
        Exception = short.MaxValue
    }


    /// <summary>
    /// The progress of a message from creation to receiving a response.
    /// </summary>
    internal enum MessageEngineDeliveryStatus
    {
        /// <summary>
        /// Message has not been sent or is flagged to resent (because socket connection failed)
        /// </summary>
        Unsent = 0,

        /// <summary>
        /// Message is in progress
        /// </summary>
        InProgress = 1,

        /// <summary>
        /// Send operation finished (Could be unsuccessful or successful)
        /// </summary>
        ResponseReceived = short.MaxValue
    }

    /// <summary>
    /// Internal message types
    /// </summary>
    internal enum MessageEngineMessageType
    {
        /// <summary>
        /// Unknown message type, usually associated with an error
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Server sends a broadcast
        /// </summary>
        BroadcastV1 = 10,

        /// <summary>
        /// Message (expecting a response, even a null response is valid). 
        /// </summary>
        MessageV1 = 20,

        /// <summary>
        /// Response to a message
        /// </summary>
        MessageResponseV1 = 30,

        /// <summary>
        /// Server is shutting down
        /// </summary>
        ServerStoppingNotificationV1 = 100,

        /// <summary>
        /// Client is disconnecting
        /// </summary>
        ClientDisconnectingNotificationV1 = 110,

        /// <summary>
        /// Clients send subscription information to the server. The server updates it's local client details
        /// </summary>
        SubscriptionChangesNotificationV1 = 200,

        /// <summary>
        /// Server sends a subscription response when a subscription notification is received from a client
        /// </summary>
        SubscriptionChangesResponseV1 = 210,

        /// <summary>
        /// Clients sent poll requests to determine if a connection is alive
        /// </summary>
        PollingRequestV1 = 300,

        /// <summary>
        /// Server sends a poll response when a poll request is received from a client
        /// </summary>
        PollingResponseV1 = 310
    }






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



/// <summary>
/// Action taken with a token
/// </summary>
internal enum TokenAction
{
    Unknown = 0,
    Add = 10,
    Modify = 20,
    Delete = 30
}


#pragma warning restore CA1062 // Validate arguments of public methods
#pragma warning restore IDE0079 // Remove unnecessary suppression


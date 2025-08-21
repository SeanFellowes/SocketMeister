using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;


namespace SocketMeister
{
    internal enum ClientDisconnectReason
    {
        /// <summary>
        /// The reason for the client disconnect is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The client disconnected because the handshake between the client and server timed out.
        /// </summary>
        HandshakeTimeout = 10,

        /// <summary>
        /// The client disconnected because the server did not respond to a poll request.
        /// </summary>
        PollingTimeout = 15,

        /// <summary>
        /// The client disconnected because the server version is not supported by this client.
        /// </summary>
        IncompatibleServerVersion = 20,

        /// <summary>
        /// The client disconnected because the server does not support this client version.
        /// </summary>
        IncompatibleClientVersion = 30,

        /// <summary>
        /// The client disconnected because the connection was reset.
        /// </summary>
        ConnectionReset = 40,

        /// <summary>
        /// The client disconnected due to a socket error.
        /// </summary>
        SocketError = 100,

        /// <summary>
        /// The socket server is not listening for connections.
        /// </summary>
        SocketConnectionRefused = 101,

        /// <summary>
        /// The socket timed out during the connection attempt.
        /// </summary>
        SocketConnectionTimeout = 102,

        /// <summary>
        /// The client disconnected because the calling program requested it.
        /// </summary>
        ClientIsStopping = 10000,

        /// <summary>
        /// The client disconnected because it received a server stopping notification.
        /// </summary>
        ServerIsStopping = 20000
    }

    /// <summary>
    /// The result of an attempt to process a message. This is included in the MessageResponse.
    /// </summary>
    internal enum MessageEngineDeliveryResult
    {
        /// <summary>
        /// The message was processed successfully.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The client or server is shutting down, so the message was not processed.
        /// </summary>
        Stopping = 2,

        /// <summary>
        /// There is no process listening for 'MessageReceived' events.
        /// </summary>
        NoMessageReceivedEventListener = 3,

        /// <summary>
        /// An exception occurred while processing the message.
        /// </summary>
        Exception = short.MaxValue
    }


    /// <summary>
    /// The status of the message.
    /// </summary>
    internal enum MessageStatus
    {
        /// <summary>
        /// The message has not been sent or is flagged to be resent (due to a socket connection failure).
        /// </summary>
        Unsent = 0,

        /// <summary>
        /// The message is in progress.
        /// </summary>
        InProgress = 1,

        /// <summary>
        /// The send operation has finished (it could be either successful or unsuccessful).
        /// </summary>
        Completed = short.MaxValue
    }


    /// <summary>
    /// Internal message types.
    /// </summary>
    internal enum MessageType
    {
        /// <summary>
        /// An unknown message type, usually associated with an error.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The server sends a broadcast.
        /// </summary>
        BroadcastV1 = 10,

        /// <summary>
        /// A message expecting a response (even a null response is valid).
        /// </summary>
        MessageV1 = 20,

        /// <summary>
        /// A response to a message.
        /// </summary>
        MessageResponseV1 = 30,

        /// <summary>
        /// The server is shutting down.
        /// </summary>
        ServerStoppingNotificationV1 = 100,

        /// <summary>
        /// The client is disconnecting.
        /// </summary>
        ClientDisconnectingNotificationV1 = 110,

        /// <summary>
        /// Clients send subscription information to the server, and the server updates its local client details.
        /// </summary>
        TokenChangesRequestV1 = 200,

        /// <summary>
        /// The server sends a subscription response when a subscription notification is received from a client.
        /// </summary>
        TokenChangesResponseV1 = 210,

        /// <summary>
        /// Clients send poll requests to determine if a connection is alive.
        /// </summary>
        PollingRequestV1 = 300,

        /// <summary>
        /// The server sends a poll response when a poll request is received from a client.
        /// </summary>
        PollingResponseV1 = 310,

        /// <summary>
        /// The server sends its version number to the client.
        /// </summary>
        Handshake1 = 400,

        /// <summary>
        /// The client sends its version number to the server after receiving Handshake1.
        /// </summary>
        Handshake2 = 410,

        /// <summary>
        /// The server sends a Handshake2Ack to the client after receiving Handshake2.
        /// </summary>
        Handshake2Ack = 411
    }


    /// <summary>
    /// The severity of a trace/log event.
    /// </summary>
#if SMISPUBLIC
    public enum Severity
#else
        internal enum Severity
#endif
    {
        /// <summary>
        /// Informational messages.
        /// </summary>
        Information = 0,

        /// <summary>
        /// Warning messages.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Error messages.
        /// </summary>
        Error = 2,

        /// <summary>
        /// Debugging messages.
        /// </summary>
        Debug = 32767
    }


    /// <summary>
    /// The category of a trace/log event.
    /// </summary>
#if SMISPUBLIC
    public enum LogEventType
#else
        internal enum LogEventType
#endif
    {
        /// <summary>
        /// Connection-related events.
        /// </summary>
        ConnectionEvent = 0,

        /// <summary>
        /// Exception events.
        /// </summary>
        Exception = 1,

        /// <summary>
        /// User-generated messages.
        /// </summary>
        UserMessage = 2,

        /// <summary>
        /// Polling-related events.
        /// </summary>
        PollingEvent = 3,

        /// <summary>
        /// Subscription synchronization events.
        /// </summary>
        SubscriptionSyncEvent = 4,

        /// <summary>
        /// Internal debugging.
        /// </summary>
        Internal = 5
    }


    /// <summary>
    /// The execution status of a SocketServer.
    /// </summary>
#if SMISPUBLIC
    public enum SocketServerStatus
#else
        internal enum SocketServerStatus
#endif
    {
        /// <summary>
        /// The service is stopped.
        /// </summary>
        Stopped = 0,

        /// <summary>
        /// The service is starting.
        /// </summary>
        Starting = 1,

        /// <summary>
        /// The service is started.
        /// </summary>
        Started = 2,

        /// <summary>
        /// The service is stopping.
        /// </summary>
        Stopping = 3
    }


    /// <summary>
    /// The action taken with a token.
    /// </summary>
    internal enum TokenAction
    {
        /// <summary>
        /// The action is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Add a token.
        /// </summary>
        Add = 10,

        /// <summary>
        /// Modify a token.
        /// </summary>
        Modify = 20,

        /// <summary>
        /// Delete a token.
        /// </summary>
        Delete = 30
    }


    /// <summary>
    /// The type of value stored in a token.
    /// </summary>
    internal enum ValueType
    {
        /// <summary>
        /// An unknown value type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A boolean value.
        /// </summary>
        BoolValue = 10,

        /// <summary>
        /// A DateTime value.
        /// </summary>
        DateTimeValue = 20,

        /// <summary>
        /// A double value.
        /// </summary>
        DoubleValue = 30,

        /// <summary>
        /// A 16-bit integer value.
        /// </summary>
        Int16Value = 40,

        /// <summary>
        /// A 32-bit integer value.
        /// </summary>
        Int32Value = 41,

        /// <summary>
        /// A 64-bit integer value.
        /// </summary>
        Int64Value = 42,

        /// <summary>
        /// A 16-bit unsigned integer value.
        /// </summary>
        UInt16Value = 50,

        /// <summary>
        /// A 32-bit unsigned integer value.
        /// </summary>
        UInt32Value = 51,

        /// <summary>
        /// A 64-bit unsigned integer value.
        /// </summary>
        UInt64Value = 52,

        /// <summary>
        /// A string value.
        /// </summary>
        StringValue = 60,

        /// <summary>
        /// A byte value.
        /// </summary>
        ByteValue = 70,

        /// <summary>
        /// A byte array value.
        /// </summary>
        ByteArrayValue = 71,

        /// <summary>
        /// A null value.
        /// </summary>
        NullValue = 99
    }


}


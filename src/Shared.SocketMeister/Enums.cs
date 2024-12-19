#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1062 // Validate arguments of public methods


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
        /// Message has been aborted (E.g. Socket closed)
        /// </summary>
        Aborted = 100,

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
        /// Delivery Receipt Acknowledgement
        /// </summary>
        MessageDeliveredAckV1 = 31,

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


}



#pragma warning restore CA1062 // Validate arguments of public methods
#pragma warning restore IDE0079 // Remove unnecessary suppression


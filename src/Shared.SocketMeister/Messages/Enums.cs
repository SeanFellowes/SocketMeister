using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal message types
    /// </summary>
    internal enum MessageTypes
    {
        /// <summary>
        /// Unknown message type, usually associated with an error
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Basic message.
        /// </summary>
        Message = 1,
        /// <summary>
        /// Request (expecting a response)
        /// </summary>
        RequestMessage = 2,
        /// <summary>
        /// Response to a client request
        /// </summary>
        ResponseMessage = 3,
        /// <summary>
        /// Server is shutting down
        /// </summary>
        ServerStoppingMessage = 4,
        /// <summary>
        /// Client is disconnecting
        /// </summary>
        ClientDisconnectMessage = 5,
        /// <summary>
        /// Clients sent poll requests to determine if a connection is alive
        /// </summary>
        PollRequest = 6,
        /// <summary>
        /// Server sends a poll response when a poll request is received from a client
        /// </summary>
        PollResponse = 7
    }

    /// <summary>
    /// Data types which are supported for parameters sent with messages.
    /// </summary>
    internal enum ParameterTypes
    {
        BoolParam = 0,
        DateTimeParam = 1,
        DoubleParam = 2,
        Int16Param = 3,
        Int32Param = 4,
        Int64Param = 5,
        UInt16Param = 6,
        UInt32Param = 7,
        UInt64Param = 8,
        StringParam = 9,
        ByteParam = 10,
        ByteArrayParam = 11,
        Null = 99
    }


    /// <summary>
    /// Status of .
    /// </summary>
    internal enum ResponseCode
    {
        Success = 0,
        GeneralException = 1,
        NoFreeSocketAsyncEventArgs = 2,
        Int16Param = 3,
        Int32Param = 4,
        Int64Param = 5,
        UInt16Param = 6,
        UInt32Param = 7,
        UInt64Param = 8,
        StringParam = 9,
        ByteParam = 10,
        ByteArrayParam = 11,
        Null = 99
    }


    /// <summary>
    /// Status of a SendReceive operation
    /// </summary>
    internal enum SendReceiveStatus
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
        /// Response has been received
        /// </summary>
        ResponseReceived = 2,
        /// <summary>
        /// Timeout occured.
        /// </summary>
        Timeout = 9
    }

}


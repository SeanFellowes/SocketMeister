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
        PollResponse = 7,

        /// <summary>
        /// Clients send subscription information to the server. The server updates it's local client details
        /// </summary>
        SubscriptionChangesRequestV1 = 20,

        /// <summary>
        /// Server sends a subscription response when a subscription request is received from a client
        /// </summary>
        SubscriptionChangesResponseV1 = 30,

        /// <summary>
        /// Server sends a message which is applicable to a specific subscription name
        /// </summary>
        SubscriptionMessageV1 = 40,

        /// <summary>
        /// Request (expecting a response). Version 2 (From SocketMeister 2.x.x.x)
        /// </summary>
        RequestMessageV1 = 100,

        /// <summary>
        /// Response to a client request
        /// </summary>
        ResponseMessageV1 = 110


    }



    /// <summary>
    /// Result of an attempt to process a request. This is included in the ResponseMessage.
    /// </summary>
    internal enum RequestResult
    {
        /// <summary>
        /// The request executed successfully
        /// </summary>
        Success = 0,
        /// <summary>
        /// The client or server is shutting down. The request could not be fulfilled.
        /// </summary>
        Stopping = 2,
        /// <summary>
        /// There is no process listening for 'RequestReceived' events.
        /// </summary>
        NoRequestProcessor = 3,
        /// <summary>
        /// An exception occured while processing the request
        /// </summary>
        Exception = short.MaxValue
    }


    /// <summary>
    /// Status of a send operation
    /// </summary>
    internal enum MessageStatus
    {
        /// <summary>
        /// Message has not been sent or is flagged to resent (because socket connection failed)
        /// </summary>
        Unsent = 0,
        /// <summary>
        /// Message is in progress
        /// </summary>
        InProgress = 1,
        ///// <summary>
        ///// Response has been received
        ///// </summary>
        //ResponseReceived = 2
        ///// <summary>
        ///// Timeout occured.
        ///// </summary>
        //Timeout = 9

        /// <summary>
        /// Send operation finished (Could be unsuccessful or successful)
        /// </summary>
        Finished = short.MaxValue

    }

}


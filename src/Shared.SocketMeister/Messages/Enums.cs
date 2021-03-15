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
        /// Server sends a subscription response when a subscription request is received from a client
        /// </summary>
        SubscriptionChangesResponseV1 = 210,

        /// <summary>
        /// Clients sent poll requests to determine if a connection is alive
        /// </summary>
        PollingV1 = 300,

        /// <summary>
        /// Server sends a poll response when a poll request is received from a client
        /// </summary>
        PollingResponseV1 = 310
    }



    /// <summary>
    /// Result of an attempt to process a message. This is included in the MessageResponse.
    /// </summary>
    internal enum MessageProcessingResult
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
        /// There is no process listening for 'MessageReceived' events.
        /// </summary>
        NoMessageProcessor = 3,
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


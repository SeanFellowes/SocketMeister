#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable IDE0063 // Use simple 'using' statement
#pragma warning disable CA1303 // Do not pass literals as localized parameters
#pragma warning disable CA1805 // Do not initialize unnecessarily
#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable CA1825 // Avoid zero-length array allocations.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using SocketMeister.Messages;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal message types
    /// </summary>
    internal enum InternalMessageType
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
    /// Result of an attempt to process a message. This is included in the MessageResponse.
    /// </summary>
    internal enum MessageResponseResult
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
    internal enum MessageProgress
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

}

#pragma warning restore CA1825 // Avoid zero-length array allocations.
#pragma warning restore CA1031 // Do not catch general exception types
#pragma warning restore CA1805 // Do not initialize unnecessarily
#pragma warning restore CA1303 // Do not pass literals as localized parameters
#pragma warning restore IDE0063 // Use simple 'using' statement
#pragma warning restore IDE0052 // Remove unread private members
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression


using System;
using System.IO;
using System.Threading;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Interface for internal messages, sent between socket clients and the socket server. Internal messages are not visible outside the socket library.
    /// </summary>
    internal interface IMessage
    {
        /// <summary>
        /// UTC datetime the message was created.
        /// </summary>
        DateTime CreatedDateTime { get; }

        /// <summary>
        /// Type of message
        /// </summary>
        MessageType MessageType { get; }

        /// <summary>
        /// Status of the message. 
        /// </summary>
        MessageStatus Status { get; }

        ///// <summary>
        ///// Timestamp in UTC when the message will timeout
        ///// </summary>
        //DateTime TimeoutDateTime { get; }

        /// <summary>
        /// Number of milliseconds after the message was created before the message should timeout
        /// </summary>
        int TimeoutMilliseconds { get; set; }
        
        /// <summary>
        /// After sending this message, the sender class will wait for a response.
        /// </summary>
        bool ContinueWaitingtForResponse { get; }

        /// <summary>
        /// Mandatory method to append binary data to the IMessage object,
        /// </summary>
        /// <param name="Writer"></param>
        void AppendBytes(BinaryWriter Writer);

        /// <summary>
        /// Response message, where applicable, so null by default
        /// </summary>
        MessageResponseV1 Response { get; }

        /// <summary>
        /// Message displosal
        /// </summary>
        void Dispose();

        void SetCompleted(MessageResponseV1 responseMessage);
        void SetInProgress();
        void TryRetrySend();


#if !NET35
        //  This is only used by some message types as a wait lock for a response to be received. Null by default
        ManualResetEventSlim ResponseReceivedEvent { get; set; }

        bool WaitForResponse(ManualResetEventSlim messageWaitHandle);
#endif


    }
}

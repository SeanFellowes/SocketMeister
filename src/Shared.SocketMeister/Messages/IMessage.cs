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
        /// Unique identifier for the message
        /// </summary>
        long MessageId { get; }

        /// <summary>
        /// Type of message
        /// </summary>
        MessageType MessageType { get; }

        /// <summary>
        /// Status of the message. 
        /// </summary>
        MessageStatus SendReceiveStatus { get; }

        /// <summary>
        /// Number of milliseconds after the message was created before the message should timeout
        /// </summary>
        int TimeoutMilliseconds { get; set; }
        
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

        void SetStatusCompleted(MessageResponseV1 responseMessage);
        void SetStatusInProgress();
        void SetStatusUnsent();


#if !NET35
        //  This is only used by some message types as a wait lock for a response to be received. Null by default
        //ManualResetEventSlim ResponseReceivedEvent { get; set; }

        bool WaitForCompleted();
#endif


    }
}

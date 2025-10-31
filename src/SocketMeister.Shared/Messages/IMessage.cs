using System;
using System.IO;

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
        /// Optional friendly name for the message, used for logging and debugging purposes.
        /// </summary>
        string FriendlyMessageName { get; }

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
        SendStatus SendReceiveStatus { get; }

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
        /// <summary>
        /// Using a blocking wait, wait for the block to be set or timeout
        /// </summary>
        void ActivateSendWaitBlocker(int TimeoutMs);
#endif


    }
}

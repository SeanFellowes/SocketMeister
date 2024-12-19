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
        MessageStatus Status { get; set; }

        /// <summary>
        /// After sending this message, the sender class will wait for a response.
        /// </summary>
        bool WaitForResponse { get; }

        /// <summary>
        /// Mandatory method to append binary data to the IMessage object,
        /// </summary>
        /// <param name="Writer"></param>
        void AppendBytes(BinaryWriter Writer);

        /// <summary>
        /// Response message, where applicable, so null by default
        /// </summary>
        IMessage Response { get; set; }


#if !NET35
        //  This is only used by some message types as a wait lock for a response to be received. Null by default
        ManualResetEventSlim ResponseReceivedEvent { get; set; }
#endif


    }
}

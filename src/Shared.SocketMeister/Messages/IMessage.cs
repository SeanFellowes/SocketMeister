using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Interface for internal messages, sent between socket clients and the socket server. Internal messages are not visible outside the socket library.
    /// </summary>
    internal interface IMessage
    {
        /// <summary>
        /// Type of message
        /// </summary>
        MessageTypes MessageType { get; }

        /// <summary>
        /// Status of the message. 
        /// </summary>
        MessageStatus Status { get; set; }

        /// <summary>
        /// Mandatory method to append binary data to the IMessage object,
        /// </summary>
        /// <param name="Writer"></param>
        void AppendBytes(BinaryWriter Writer);

    }
}

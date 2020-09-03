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
        /// Status of a 'SendRequest()' operation, used only on the SocketClient, to track: If messages have been sent, a response has been received, or an error/timeout.
        /// </summary>
        SendStatus SendStatus { get; set; }

        ///// <summary>
        ///// Number of milliseconds to wait before a timeout will occur.
        ///// </summary>
        //int TimeoutMilliseconds { get; }

        /// <summary>
        /// Mandatory method to append binary data to the IMessage object,
        /// </summary>
        /// <param name="Writer"></param>
        void AppendBytes(BinaryWriter Writer);

    }
}

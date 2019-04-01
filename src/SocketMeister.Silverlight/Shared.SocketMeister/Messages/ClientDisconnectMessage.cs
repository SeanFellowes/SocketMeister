using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Sent when a client is disconnecting
    /// </summary>
    internal class ClientDisconnectMessage : MessageBase, IMessage
    {
        private readonly object _lock = new object();

        public ClientDisconnectMessage() : base(MessageTypes.ClientDisconnectMessage)
        {
        }

        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public ClientDisconnectMessage(BinaryReader Reader) : base(MessageTypes.ClientDisconnectMessage)
        {
        }

        public void AppendBytes(BinaryWriter Writer)
        {
        }
    }
}

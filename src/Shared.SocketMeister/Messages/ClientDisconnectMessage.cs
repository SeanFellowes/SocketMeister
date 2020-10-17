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
        /// <summary>
        /// Increment this and add deserialization code when changing the serialized format.
        /// </summary>
        private const int SERIALIZER_VERSION = 1;

        public ClientDisconnectMessage() : base(MessageTypes.ClientDisconnectMessage, SERIALIZER_VERSION)
        {
        }

        ///// <summary>
        ///// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        ///// </summary>
        ///// <param name="Reader">Binary Reader</param>
        //public ClientDisconnectMessage(BinaryReader Reader) : base(MessageTypes.ClientDisconnectMessage)
        //{
        //}

        public void AppendBytes(BinaryWriter Writer)
        {
        }
    }
}

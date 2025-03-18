using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: SocketServer sends SocketClient a Handshake2Ack after receiving a
    /// Handshake2 message from the client.
    /// Introduced in version 5 of SocketMeister for robust handshaking.
    /// </summary>
    internal class Handshake2Ack : MessageBase, IMessage
    {
        public Handshake2Ack() : base(MessageType.Handshake2Ack, messageId: 0)
        {
        }

        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public Handshake2Ack(BinaryReader Reader) : base(MessageType.Handshake2Ack, messageId: 0)
        {
        }

        public void AppendBytes(BinaryWriter Writer)
        {
        }
    }
}


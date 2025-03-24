using System.IO;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: For testing purposes, servers sends a message type unknown to the client
    /// Introduced in version 10 of SocketMeister testing unsupported messages.
    /// </summary>
    internal class UnsupportedMessageFromServer : MessageBase, IMessage
    {
        private readonly int _socketServerVersion;

        public UnsupportedMessageFromServer(int socketServerVersion, string clientId) : base(MessageType.Handshake1, messageId: 0)
        {
            _socketServerVersion = socketServerVersion;
        }

        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public UnsupportedMessageFromServer(BinaryReader Reader) : base(MessageType.Handshake1, messageId: 0)
        {
            _socketServerVersion = Reader.ReadInt32();
        }

        public int SocketServerVersion => _socketServerVersion;

        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_socketServerVersion);
        }
    }
}

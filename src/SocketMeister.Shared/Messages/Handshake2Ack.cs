using System.IO;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: SocketServer sends SocketClient a Handshake2Ack after receiving a
    /// Handshake2 message from the client.
    /// Introduced in version 10 of SocketMeister for robust handshaking.
    /// </summary>
    internal class Handshake2Ack : MessageBase, IMessage
    {
        private readonly bool _serverSupportsClientVersion;

        public Handshake2Ack(bool serverSupportsClientVersion) : base(MessageType.Handshake2Ack, messageId: 0, nameof(Handshake2Ack))
        {
            _serverSupportsClientVersion = serverSupportsClientVersion;
        }

        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public Handshake2Ack(BinaryReader Reader) : base(MessageType.Handshake2Ack, messageId: 0, nameof(Handshake2Ack))
        {
            _serverSupportsClientVersion = Reader.ReadBoolean();
        }

        public bool ServerSupportsClientVersion => _serverSupportsClientVersion;


        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_serverSupportsClientVersion);
        }
    }
}


using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: SocketClient sends the server the version number of the SocketClient.
    /// This is sent when the client receives the version number of the SocketServer (Handshake1).
    /// Introduced in version 4 of SocketMeister for robust handshaking.
    /// </summary>
    internal class Handshake2 : MessageBase, IMessage
    {
        private readonly int _clientSocketMeisterVersion;

        public Handshake2(int clientSocketMeisterVersion) : base(MessageType.Handshake2, messageId: 0)
        {
            _clientSocketMeisterVersion = clientSocketMeisterVersion;
        }

        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public Handshake2(BinaryReader Reader) : base(MessageType.Handshake2, messageId: 0)
        {
            _clientSocketMeisterVersion = Reader.ReadInt32();
        }

        public int ClientSocketMeisterVersion => _clientSocketMeisterVersion;

        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_clientSocketMeisterVersion);
        }
    }
}


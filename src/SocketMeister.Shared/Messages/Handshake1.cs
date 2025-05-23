﻿using System.IO;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: SocketServer sends the client the version number of the SocketServer 
    /// when it is ready to receive data. 
    /// Introduced in version 10 of SocketMeister for robust handshaking.
    /// </summary>
    internal class Handshake1 : MessageBase, IMessage
    {
        private readonly int _serverSocketMeisterVersion;
        private readonly string _clientId;

        public Handshake1(int serverSocketMeisterVersion, string clientId) : base(MessageType.Handshake1, messageId: 0)
        {
            _serverSocketMeisterVersion = serverSocketMeisterVersion;
            _clientId = clientId;
        }

        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public Handshake1(BinaryReader Reader) : base(MessageType.Handshake1, messageId: 0)
        {
            _serverSocketMeisterVersion = Reader.ReadInt32();
            _clientId = Reader.ReadString();
        }

        public string ClientId => _clientId;

        public int ServerSocketMeisterVersion => _serverSocketMeisterVersion;

        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_serverSocketMeisterVersion);
            Writer.Write(_clientId);
        }
    }
}

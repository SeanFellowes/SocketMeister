using System.IO;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: SocketClient sends client related information the server as part of
    /// the handshake process.
    /// This Handshake2 message is sent when the client receives Handshake1 from the server.
    /// Introduced in version 10 of SocketMeister for robust handshaking.
    /// </summary>
    internal class Handshake2 : MessageBase, IMessage
    {
        private readonly bool _clientSupportsServerVersion;
        private readonly int _clientSocketMeisterVersion;
        private readonly string _friendlyName;
        private readonly byte[] _subscriptionBytes;


        public Handshake2(int clientSocketMeisterVersion, string friendlyName, byte[] subscriptionBytes, bool clientSuportsServerVersion) : base(MessageType.Handshake2, messageId: 0)
        {
            _clientSocketMeisterVersion = clientSocketMeisterVersion;
            _friendlyName = friendlyName;
            _subscriptionBytes = subscriptionBytes;
            _clientSupportsServerVersion = clientSuportsServerVersion;
        }

        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public Handshake2(BinaryReader Reader) : base(MessageType.Handshake2, messageId: 0)
        {
            _clientSupportsServerVersion = Reader.ReadBoolean();
            _clientSocketMeisterVersion = Reader.ReadInt32();
            if (Reader.ReadBoolean())
            {
                _friendlyName = Reader.ReadString();
            }
            if (Reader.ReadBoolean())
            {
                _subscriptionBytes = Reader.ReadBytes(Reader.ReadInt32());
            }
        }

        public byte[] ChangeBytes => _subscriptionBytes;

        public int ClientSocketMeisterVersion => _clientSocketMeisterVersion;

        public string FriendlyName => _friendlyName;

        public bool ClientSupportsServerVersion => _clientSupportsServerVersion;

        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_clientSupportsServerVersion);
            Writer.Write(_clientSocketMeisterVersion);
            if (_friendlyName != null)
            {
                Writer.Write(true);
                Writer.Write(_friendlyName);
            }
            else
            {
                Writer.Write(false);
            }
            if (_subscriptionBytes != null)
            {
                Writer.Write(true);
                Writer.Write(_subscriptionBytes.Length);
                Writer.Write(_subscriptionBytes);
            }
            else
            {
                Writer.Write(false);
            }
        }
    }
}


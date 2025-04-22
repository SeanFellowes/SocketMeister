using System.IO;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Sent when a client is disconnecting
    /// </summary>
    internal class ClientDisconnectingNotificationV1 : MessageBase, IMessage
    {
        private string _clientMessage;
        private ClientDisconnectReason _disconnectReason;

        public ClientDisconnectingNotificationV1(ClientDisconnectReason disconnectReason, string clientMessage) : base(MessageType.ClientDisconnectingNotificationV1, messageId: 0)
        {
            _disconnectReason = disconnectReason;
            _clientMessage = clientMessage;
        }

        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public ClientDisconnectingNotificationV1(BinaryReader Reader) : base(MessageType.ClientDisconnectingNotificationV1, messageId: 0)
        {
            _disconnectReason = (ClientDisconnectReason)Reader.ReadInt16();
            if (Reader.ReadBoolean())
            {
                _clientMessage = Reader.ReadString();
            }
        }

        public string ClientMessage => _clientMessage;

        public ClientDisconnectReason DisconnectReason => _disconnectReason;

        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write((short)_disconnectReason);
            if (_clientMessage != null)
            {
                Writer.Write(true);
                Writer.Write(_clientMessage);
            }
            else
            {
                Writer.Write(false);
            }
        }
    }
}

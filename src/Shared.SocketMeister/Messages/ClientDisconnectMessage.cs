using System.IO;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Sent when a client is disconnecting
    /// </summary>
    internal class ClientDisconnectMessage : MessageBase, IMessage
    {
        public ClientDisconnectMessage() : base(MessageTypes.ClientDisconnectMessage)
        {
        }

        public void AppendBytes(BinaryWriter Writer)
        {
        }
    }
}

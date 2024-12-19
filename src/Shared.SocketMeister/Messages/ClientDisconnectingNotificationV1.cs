using System.IO;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Sent when a client is disconnecting
    /// </summary>
    internal class ClientDisconnectingNotificationV1 : MessageBase, IMessage
    {
        public ClientDisconnectingNotificationV1() : base(MessageType.ClientDisconnectingNotificationV1, waitForResponse: false)
        {
        }

        public void AppendBytes(BinaryWriter Writer)
        {
        }
    }
}

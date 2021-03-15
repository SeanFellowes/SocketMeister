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
        public ClientDisconnectMessage() : base(MessageTypes.ClientDisconnectingNotificationV1)
        {
        }

        public void AppendBytes(BinaryWriter Writer)
        {
        }
    }
}

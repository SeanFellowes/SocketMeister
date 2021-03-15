using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Sent when a client is disconnecting
    /// </summary>
    internal class ClientDisconnectingNotificationV1 : MessageBase, IMessage
    {
        public ClientDisconnectingNotificationV1() : base(MessageEngineMessageType.ClientDisconnectingNotificationV1)
        {
        }

        public void AppendBytes(BinaryWriter Writer)
        {
        }
    }
}

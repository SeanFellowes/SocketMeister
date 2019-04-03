using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: Socket server sends a response to a 'PollRequest' message to indicate (to the client) that the server is alive.
    /// </summary>
    internal class PollResponse : MessageBase, IMessage
    {
        public PollResponse() : base(MessageTypes.PollResponse) { }

        public void AppendBytes(BinaryWriter Writer)
        {
        }
    }
}

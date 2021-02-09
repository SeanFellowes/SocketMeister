using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: Socket server sends a response to a 'PollRequest' message to indicate (to the client) that the server is alive.
    /// </summary>
    internal class SubscriptionResponse : MessageBase, IMessage
    {
        public SubscriptionResponse() : base(MessageTypes.SubscriptionResponse) { }

        ///// <summary>
        ///// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        ///// </summary>
        ///// <param name="Reader">Binary Reader</param>
        //public PollResponse(BinaryReader Reader) : base(MessageTypes.PollResponse)
        //{
        //}

        public void AppendBytes(BinaryWriter Writer)
        {
        }
    }
}

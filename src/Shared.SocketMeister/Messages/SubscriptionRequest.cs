using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: Socket client regulary sends subscription messages to the server.
    /// </summary>
    internal class SubscriptionRequest : MessageBase, IMessage
    {
        private readonly byte[] _changeBytes;

        public SubscriptionRequest(byte[] ChangeBytes) : base(MessageTypes.SubscriptionRequest) 
        {
            _changeBytes = ChangeBytes;
        }

        ///// <summary>
        ///// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        ///// </summary>
        ///// <param name="Reader">Binary Reader</param>
        //public SubscriptionRequest(BinaryReader Reader) : base(MessageTypes.PollRequest)
        //{
        //}

        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_changeBytes);
        }
    }
}

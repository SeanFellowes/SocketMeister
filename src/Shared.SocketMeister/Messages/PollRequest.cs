using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: Socket client regulary sends a poll request to the server to check that the server is alive.
    /// </summary>
    internal class PollRequest : MessageBase, IMessage
    {
        /// <summary>
        /// Increment this and add deserialization code when changing the serialized format.
        /// </summary>
        private const int SERIALIZER_VERSION = 1;

        public PollRequest() : base(MessageTypes.PollRequest, SERIALIZER_VERSION) { }

        ///// <summary>
        ///// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        ///// </summary>
        ///// <param name="Reader">Binary Reader</param>
        //public PollRequest(BinaryReader Reader) : base(MessageTypes.PollRequest)
        //{
        //}

        public void AppendBytes(BinaryWriter Writer)
        {
        }
    }
}

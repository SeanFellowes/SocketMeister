#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1062 // Validate arguments of public methods
#pragma warning disable CA1812 // Validate arguments of public methods

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: Socket server sends a response to a 'PollRequest' message to indicate (to the client) that the server is alive.
    /// </summary>
    internal class PollingResponseV1 : MessageBase, IMessage
    {
        public PollingResponseV1() : base(MessageEngineMessageType.PollingResponseV1) { }

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

#pragma warning restore CA1812 // Validate arguments of public methods
#pragma warning restore CA1062 // Validate arguments of public methods
#pragma warning restore IDE0079 // Remove unnecessary suppression


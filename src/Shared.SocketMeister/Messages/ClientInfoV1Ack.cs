using System.IO;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: Socket server acknowledgement that client info was received from a newly connected client.
    /// </summary>
    internal class ClientInfoV1Ack : MessageBase, IMessage
    {
        public ClientInfoV1Ack() : base(MessageType.ClientInfoV1Ack) { }

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


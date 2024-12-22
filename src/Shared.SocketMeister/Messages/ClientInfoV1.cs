using System.IO;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: Socket client sends client info to the server after connection.
    /// </summary>
    internal class ClientInfoV1 : MessageBase, IMessage
    {
        public ClientInfoV1() : base(MessageType.ClientInfoV1) { }

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

using System.IO;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: Socket client regulary sends a poll request to the server to check that the server is alive.
    /// </summary>
    internal class PollingRequestV1 : MessageBase, IMessage
    {
        public PollingRequestV1() : base(MessageType.PollingRequestV1, messageId: 0, nameof(PollingRequestV1)) { }

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

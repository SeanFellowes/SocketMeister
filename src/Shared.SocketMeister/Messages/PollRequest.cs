using System.IO;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: Socket client regulary sends a poll request to the server to check that the server is alive.
    /// </summary>
    internal class PollRequest : MessageBase, IMessage
    {
        public PollRequest() : base(MessageTypes.PollRequest) { }

        public void AppendBytes(BinaryWriter writer)
        {
        }
    }
}

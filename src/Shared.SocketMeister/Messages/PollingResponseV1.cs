using System.IO;

namespace SocketMeister.Messages
{
    /// <summary>
    /// Internal Message: Socket server sends a response to a 'PollRequest' message to indicate (to the client) that the server is alive.
    /// </summary>
    internal class PollingResponseV1 : MessageBase, IMessage
    {
        public PollingResponseV1() : base(MessageType.PollingResponseV1) { }

        public void AppendBytes(BinaryWriter Writer) { }
    }
}


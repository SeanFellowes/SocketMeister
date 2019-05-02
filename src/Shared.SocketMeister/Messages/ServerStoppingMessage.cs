using System.IO;

namespace SocketMeister.Messages
{
    internal class ServerStoppingMessage : MessageBase, IMessage
    {
        //  INTERNAL (NOT SENT IN MESSAGE DATA)
        private readonly int maxWaitMilliseconds;

        public ServerStoppingMessage(int maxWaitMilliseconds) : base(MessageTypes.ServerStoppingMessage)
        {
            this.maxWaitMilliseconds = maxWaitMilliseconds;
        }

        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="reader">Binary Reader</param>
        public ServerStoppingMessage(BinaryReader reader) : base(MessageTypes.ServerStoppingMessage)
        {
            maxWaitMilliseconds = reader.ReadInt32();
        }

        /// <summary>
        /// The maximum number of milliseconds the client should wait for open requests to be completed. The server will not send back responses for open requests after this time.
        /// </summary>
        public int MaxWaitMilliseconds
        {
            get { return maxWaitMilliseconds; }
        }



        public void AppendBytes(BinaryWriter writer)
        {
            writer.Write(maxWaitMilliseconds);
        }





    }
}

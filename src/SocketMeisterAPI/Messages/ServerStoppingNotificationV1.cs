using System.IO;

namespace SocketMeister.Messages
{
    internal class ServerStoppingNotificationV1 : MessageBase, IMessage
    {
        //  INTERNAL (NOT SENT IN MESSAGE DATA)
        private readonly int _maxWaitMilliseconds;

        public ServerStoppingNotificationV1(int MaxWaitMilliseconds) : base(MessageType.ServerStoppingNotificationV1, messageId: 0)
        {
            _maxWaitMilliseconds = MaxWaitMilliseconds;
        }

        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public ServerStoppingNotificationV1(BinaryReader Reader) : base(MessageType.ServerStoppingNotificationV1, messageId: 0)
        {
            _maxWaitMilliseconds = Reader.ReadInt32();
        }

        /// <summary>
        /// The maximum number of milliseconds the client should wait for unresponded messages to be completed. The server will not send back responses for unresponded messages after this time.
        /// </summary>
        public int MaxWaitMilliseconds => _maxWaitMilliseconds;



        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_maxWaitMilliseconds);
        }





    }
}

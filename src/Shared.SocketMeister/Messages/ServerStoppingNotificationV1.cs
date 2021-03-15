using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SocketMeister.Messages
{
    internal class ServerStoppingNotificationV1 : MessageBase, IMessage
    {
        //  INTERNAL (NOT SENT IN MESSAGE DATA)
        private readonly int _maxWaitMilliseconds;

        public ServerStoppingNotificationV1(int MaxWaitMilliseconds) : base(MessageEngineMessageType.ServerStoppingNotificationV1)
        {
            _maxWaitMilliseconds = MaxWaitMilliseconds;
        }

        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public ServerStoppingNotificationV1(BinaryReader Reader) : base(MessageEngineMessageType.ServerStoppingNotificationV1)
        {
            _maxWaitMilliseconds = Reader.ReadInt32();
        }

        /// <summary>
        /// The maximum number of milliseconds the client should wait for unresponded messages to be completed. The server will not send back responses for unresponded messages after this time.
        /// </summary>
        public int MaxWaitMilliseconds
        {
            get { return _maxWaitMilliseconds; }
        }



        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_maxWaitMilliseconds);
        }





    }
}

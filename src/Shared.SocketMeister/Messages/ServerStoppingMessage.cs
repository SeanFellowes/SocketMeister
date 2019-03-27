using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SocketMeister.Messages
{
    internal class ServerStoppingMessage : MessageBase, IMessage
    {
        //  INTERNAL (NOT SENT IN MESSAGE DATA)
        private readonly object _lock = new object();
        private readonly int _maxWaitMilliseconds;

        public ServerStoppingMessage(int MaxWaitMilliseconds) : base(MessageTypes.ServerStoppingMessage)
        {
            _maxWaitMilliseconds = MaxWaitMilliseconds;
        }

        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public ServerStoppingMessage(BinaryReader Reader) : base(MessageTypes.ServerStoppingMessage)
        {
            _maxWaitMilliseconds = Reader.ReadInt32();
        }

        /// <summary>
        /// The maximum number of milliseconds the client should wait for open requests to be completed. The server will not send back responses for open requests after this time.
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

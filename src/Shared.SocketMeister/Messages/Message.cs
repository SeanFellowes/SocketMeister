using System;
using System.IO;


namespace SocketMeister.Messages
{
#if !SILVERLIGHT && !SMNOSERVER
    internal partial class Message
    {
        private SocketServer.Client _remoteClient = null;

        /// <summary>
        /// Only populated for messages received on the server. This is the remote client which sent the message
        /// </summary>
        internal SocketServer.Client RemoteClient
        {
            get { lock (_lock) { return _remoteClient; } }
            set { lock (_lock) { _remoteClient = value; } }
        }
    }
#endif

    /// <summary>
    /// A basic message
    /// </summary>
    internal partial class Message : MessageBase, IMessage
    {
        //  REQUEST VARIABLES
        private readonly byte[] _parameterBytes = null;
        private readonly object[] _parameters = null;

        //  INTERNAL (NOT SENT IN MESSAGE DATA)
        private readonly object _lock = new object();

        /// <summary>
        /// RequestMessage constructor
        /// </summary>
        /// <param name="Parameters">Array of parameters to send with the request. There must be at least 1 parameter.</param>
        /// <param name="TimeoutMilliseconds">The maximum number of milliseconds to wait for a response before timing out.</param>
        public Message(object[] Parameters, int TimeoutMilliseconds) : base(MessageTypes.Message, TimeoutMilliseconds)
        {
            _parameters = Parameters;
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                SerializeParameters(writer, Parameters);
                using (BinaryReader reader = new BinaryReader(writer.BaseStream))
                {
                    reader.BaseStream.Position = 0;
                    _parameterBytes = reader.ReadBytes((Convert.ToInt32(reader.BaseStream.Length)));
                }
            }
        }


        internal Message(BinaryReader bR) : base(MessageTypes.Message)
        {
            _parameters = DeserializeParameters(bR);
        }


        /// <summary>
        /// Parameters provided with this request
        /// </summary>
        public object[] Parameters
        {
            get { return _parameters; }
        }

        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_parameterBytes);
        }
    }
}

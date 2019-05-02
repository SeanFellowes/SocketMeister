using System;
using System.IO;


namespace SocketMeister.Messages
{
#if !SILVERLIGHT && !SMNOSERVER
    internal partial class Message
    {
        private SocketServer.Client remoteClient = null;

        /// <summary>
        /// Only populated for messages received on the server. This is the remote client which sent the message
        /// </summary>
        internal SocketServer.Client RemoteClient
        {
            get { lock (classLock) { return remoteClient; } }
            set { lock (classLock) { remoteClient = value; } }
        }
    }
#endif

    /// <summary>
    /// A basic message
    /// </summary>
    internal partial class Message : MessageBase, IMessage
    {
        //  REQUEST VARIABLES
        private readonly byte[] parameterBytes = null;
        private readonly object[] parameters = null;

        //  INTERNAL (NOT SENT IN MESSAGE DATA)
        private readonly object classLock = new object();

        /// <summary>
        /// RequestMessage constructor
        /// </summary>
        /// <param name="parameters">Array of parameters to send with the request. There must be at least 1 parameter.</param>
        /// <param name="timeoutMilliseconds">The maximum number of milliseconds to wait for a response before timing out.</param>
        public Message(object[] parameters, int timeoutMilliseconds) : base(MessageTypes.Message, timeoutMilliseconds)
        {
            this.parameters = parameters;
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                SerializeParameters(writer, parameters);
                using (BinaryReader reader = new BinaryReader(writer.BaseStream))
                {
                    reader.BaseStream.Position = 0;
                    parameterBytes = reader.ReadBytes((Convert.ToInt32(reader.BaseStream.Length)));
                }
            }
        }


        internal Message(BinaryReader reader) : base(MessageTypes.Message)
        {
            parameters = DeserializeParameters(reader);
        }


        /// <summary>
        /// Parameters provided with this request
        /// </summary>
        public object[] Parameters
        {
            get { return parameters; }
        }

        public void AppendBytes(BinaryWriter writer)
        {
            writer.Write(parameterBytes);
        }
    }
}

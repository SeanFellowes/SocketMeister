using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;

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
        private readonly DateTime _timeout;
        private readonly int _timeoutMilliseconds;

        //  INTERNAL (NOT SENT IN MESSAGE DATA)
        private readonly object _lock = new object();

        /// <summary>
        /// RequestMessage constructor
        /// </summary>
        /// <param name="Parameters">Array of parameters to send with the request. There must be at least 1 parameter.</param>
        /// <param name="TimeoutMilliseconds">The maximum number of milliseconds to wait for a response before timing out.</param>
        public Message(object[] Parameters, int TimeoutMilliseconds) : base(MessageTypes.OLDMessage, 1)
        {
            _parameters = Parameters;
            _timeoutMilliseconds = TimeoutMilliseconds;
            _timeout = DateTime.Now.AddMilliseconds(TimeoutMilliseconds);
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                writer.Write(_timeoutMilliseconds);
                Serializer.SerializeParameters(writer, Parameters);
                using (BinaryReader reader = new BinaryReader(writer.BaseStream))
                {
                    reader.BaseStream.Position = 0;
                    _parameterBytes = reader.ReadBytes((Convert.ToInt32(reader.BaseStream.Length)));
                }
            }
        }


        internal Message(BinaryReader bR, int SerializationVersion) : base(MessageTypes.OLDMessage, SerializationVersion)
        {
            if (SerializationVersion == 1 || SerializationVersion == 2)
            {
                _timeoutMilliseconds = TimeoutMilliseconds;
                _parameters = Serializer.DeserializeParameters(bR);
            }
            else
            {
                throw new PlatformNotSupportedException("Deserializer does not exist for version " + SerializationVersion + " of message type " + nameof(Message) + " in this version of this assembly (" + Assembly.GetExecutingAssembly().FullName + ")");
            }
        }

        /// <summary>
        /// Whether the RequestMessage has timed out
        /// </summary>
        public bool IsTimeout
        {
            get
            {
                if (DateTime.Now > _timeout) return true;
                else return false;
            }
        }


        /// <summary>
        /// Parameters provided with this request
        /// </summary>
        public object[] Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// Number of milliseconds to wait before a timeout will occur.
        /// </summary>
        public int TimeoutMilliseconds { get { return _timeoutMilliseconds; } }


        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_parameterBytes);
        }
    }
}

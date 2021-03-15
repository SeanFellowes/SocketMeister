//#pragma warning disable IDE0079 // Remove unnecessary suppression
//#pragma warning disable IDE0090 // Use 'new(...)'
//#pragma warning disable IDE0063 // Use simple 'using' statement
//#pragma warning disable CA1805 // Do not initialize unnecessarily

//using System;
//using System.IO;
//using System.Net.Sockets;


//namespace SocketMeister.Messages
//{
//#if !SILVERLIGHT && !SMNOSERVER && !NET35 && !NET20
//    internal partial class Message
//    {
//        //  INTERNAL (NOT SENT IN MESSAGE DATA)
//        private readonly object _lock = new object();
//        private SocketServer.Client _remoteClient = null;

//        /// <summary>
//        /// Only populated for messages received on the server. This is the remote client which sent the message
//        /// </summary>
//        internal SocketServer.Client RemoteClient
//        {
//            get { lock (_lock) { return _remoteClient; } }
//            set { lock (_lock) { _remoteClient = value; } }
//        }
//    }
//#endif

//    /// <summary>
//    /// A basic message
//    /// </summary>
//    internal partial class Message : MessageBase, IMessage
//    {
//        //  REQUEST VARIABLES
//        private readonly byte[] _parameterBytes = null;
//        private readonly object[] _parameters = null;
//        private readonly DateTime _timeout;
//        private readonly int _timeoutMilliseconds;

//        /// <summary>
//        /// RequestMessage constructor
//        /// </summary>
//        /// <param name="Parameters">Array of parameters to send with the request. There must be at least 1 parameter.</param>
//        /// <param name="TimeoutMilliseconds">The maximum number of milliseconds to wait for a response before timing out.</param>
//        public Message(object[] Parameters, int TimeoutMilliseconds) : base(MessageTypes.Message)
//        {
//            _parameters = Parameters;
//            _timeoutMilliseconds = TimeoutMilliseconds;
//            _timeout = DateTime.Now.AddMilliseconds(TimeoutMilliseconds);
//            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
//            {
//                writer.Write(_timeoutMilliseconds);
//                Serializer.SerializeParameters(writer, Parameters);
//                using (BinaryReader reader = new BinaryReader(writer.BaseStream))
//                {
//                    reader.BaseStream.Position = 0;
//                    _parameterBytes = reader.ReadBytes((Convert.ToInt32(reader.BaseStream.Length)));
//                }
//            }
//        }


//        internal Message(BinaryReader bR) : base(MessageTypes.Message)
//        {
//            _timeoutMilliseconds = bR.ReadInt32();
//            _parameters = Serializer.DeserializeParameters(bR);
//        }

//        /// <summary>
//        /// Whether the RequestMessage has timed out
//        /// </summary>
//        public bool IsTimeout
//        {
//            get
//            {
//                if (DateTime.Now > _timeout) return true;
//                else return false;
//            }
//        }


//        /// <summary>
//        /// Parameters provided with this request
//        /// </summary>
//        public object[] Parameters
//        {
//            get { return _parameters; }
//        }

//        /// <summary>
//        /// Number of milliseconds to wait before a timeout will occur.
//        /// </summary>
//        public int TimeoutMilliseconds { get { return _timeoutMilliseconds; } }


//        public void AppendBytes(BinaryWriter Writer)
//        {
//            Writer.Write(_parameterBytes);
//        }
//    }
//}

//#pragma warning restore CA1805 // Do not initialize unnecessarily
//#pragma warning restore IDE0063 // Use simple 'using' statement
//#pragma warning restore IDE0090 // Use 'new(...)'
//#pragma warning restore IDE0079 // Remove unnecessary suppression


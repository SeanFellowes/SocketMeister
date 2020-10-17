using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace SocketMeister.Messages
{
#if !SILVERLIGHT && !SMNOSERVER
    internal partial class RequestMessage : MessageBase
    {
        private SocketServer.Client _remoteClient = null;

        /// <summary>
        /// The remote client which sent this RequestMessage (value null on SocketClient)
        /// </summary>
        internal SocketServer.Client RemoteClient
        {
            get { lock (Lock) { return _remoteClient; } }
            set { lock (Lock) { _remoteClient = value; } }
        }
    }
#endif

    /// <summary>
    /// A request, sent from socket client to socket server. A response is expected and will cause problems if it is not sent.
    /// </summary>
    internal partial class RequestMessage : MessageBase, IMessage
    {
        /// <summary>
        /// Increment this and add deserialization code when changing the serialized format.
        /// </summary>
        private const int SERIALIZER_VERSION = 2;

        //  REQUEST ID
        private static long _maxRequestId;
        private static readonly object _lockMaxRequestId = new object();

        //  REQUEST VARIABLES
        private readonly bool _isLongPolling;
        private bool _isResponseReceived;
        private readonly byte[] _parameterBytes = null;
        private readonly object[] _parameters = null;
        private readonly long _requestId;
        private readonly DateTime _timeout;
        private readonly int _timeoutMilliseconds;

        //  INTERNAL (NOT SENT IN MESSAGE DATA)
        private ResponseMessage _response = null;

        /// <summary>
        /// RequestMessage constructor
        /// </summary>
        /// <param name="Parameters">Array of parameters to send with the request. There must be at least 1 parameter.</param>
        /// <param name="TimeoutMilliseconds">The maximum number of milliseconds to wait for a response before timing out.</param>
        /// <param name="IsLongPolling">The maximum number of milliseconds to wait for a response before timing out.</param>
        public RequestMessage(object[] Parameters, int TimeoutMilliseconds, bool IsLongPolling = false) : base(MessageTypes.RequestMessage, SERIALIZER_VERSION)
        {
            _parameters = Parameters;
            _timeoutMilliseconds = TimeoutMilliseconds;
            _timeout = DateTime.Now.AddMilliseconds(TimeoutMilliseconds);
            _isLongPolling = IsLongPolling;

            //  CREATE A REQUEST ID
            lock (_lockMaxRequestId)
            {
                if (_maxRequestId + 1 > long.MaxValue) _maxRequestId = 1;
                else _maxRequestId += 1;
                _requestId = _maxRequestId;
            }

            //  SERIALIZE REQUEST MESSAGE
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                writer.Write(_requestId);
                writer.Write(_timeoutMilliseconds);
                writer.Write(_isLongPolling);
                Serializer.SerializeParameters(writer, Parameters);
                using (BinaryReader reader = new BinaryReader(writer.BaseStream))
                {
                    reader.BaseStream.Position = 0;
                    _parameterBytes = reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length));
                }
            }
        }


        internal RequestMessage(BinaryReader bR, int SerializationVersion) : base(MessageTypes.RequestMessage, SerializationVersion)
        {
            if (SerializationVersion == 1)
            {
                //  NOT SERIALIZED IN SV1
                _timeoutMilliseconds = 60000;       //  NOT IN VERSION 1

                _requestId = bR.ReadInt64();
                _isLongPolling = bR.ReadBoolean();
                _parameters = Serializer.DeserializeParameters(bR);
            }
            else if (SerializationVersion == 2)
            {
                _requestId = bR.ReadInt64();
                _timeoutMilliseconds = bR.ReadInt32();
                _isLongPolling = bR.ReadBoolean();
                _parameters = Serializer.DeserializeParameters(bR);
            }
            else
            {
                throw new PlatformNotSupportedException("Deserializer does not exist for version " + SerializationVersion + " of message type " + nameof(RequestMessage) + " in this version of this assembly (" + Assembly.GetExecutingAssembly().FullName + ")");
            }

            //  SETUP TIMEOUT
            _timeout = DateTime.Now.AddMilliseconds(TimeoutMilliseconds);
        }


        /// <summary>
        /// Parameters provided with this request
        /// </summary>
        public object[] Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// True is this request is long polling on the server side. Long polling requests will be closed immediately in the event of a close from either the client or server side.
        /// </summary>
        public bool IsLongPolling
        {
            get { return _isLongPolling; }
        }

        public bool IsResponseReceived
        {
            get { lock (Lock) { return _isResponseReceived; } }
            set { lock (Lock) { _isResponseReceived = value; } }
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
        /// Identifier assigned to this request when it was created (usually when created at the client)
        /// </summary>
        public long RequestId
        {
            get { return _requestId; }
        }

        public ResponseMessage Response
        {
            get { lock (Lock) { return _response; } }
            set { lock (Lock) { _response = value; } }
        }


        /// <summary>
        /// Number of milliseconds to wait before a timeout will occur.
        /// </summary>
        public int TimeoutMilliseconds { get { return _timeoutMilliseconds; } }

        /// <summary>
        /// Whether the method SendReceive(RequestMessage Request) should continute trying
        /// </summary>
        public bool TrySendReceive
        {
            get
            {
                if (Status == MessageStatus.Unsent) return true;
                else return WaitForResponse;
            }
        }

        /// <summary>
        /// Whether a SendReceive process should continue waiting for a response
        /// </summary>
        public bool WaitForResponse
        {
            get
            {
                if (IsAborted) return false;
                if (IsTimeout) return false;
                else if (Response != null) return false;
                else if (Status == MessageStatus.InProgress) return true;
                else return false;
            }
        }


        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_parameterBytes);
        }


    }
}

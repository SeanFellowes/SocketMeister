using System;
using System.IO;
using System.Net.Sockets;


namespace SocketMeister.Messages
{
#if !SILVERLIGHT && !SMNOSERVER
    internal partial class RequestMessage
    {
        private SocketServer.Client _remoteClient = null;

        /// <summary>
        /// The remote client which sent this RequestMessage (value null on SocketClient)
        /// </summary>
        internal SocketServer.Client RemoteClient
        {
            get { lock (_lock) { return _remoteClient; } }
            set { lock (_lock) { _remoteClient = value; } }
        }
    }
#endif

    /// <summary>
    /// A request, sent from socket client to socket server. A response is expected and will cause problems if it is not sent.
    /// </summary>
    internal partial class RequestMessage : MessageBase, IMessage
    {
        //  REQUEST ID
        private static long _maxRequestId = 0;
        private static readonly object _lockMaxRequestId = new object();

        //  REQUEST VARIABLES
        private readonly bool _isLongPolling = false;
        private readonly byte[] _parameterBytes = null;
        private readonly object[] _parameters = null;
        private readonly long _requestId;

        //  INTERNAL (NOT SENT IN MESSAGE DATA)
        private readonly object _lock = new object();
        private ResponseMessage _response = null;


        /// <summary>
        /// RequestMessage constructor
        /// </summary>
        /// <param name="Parameters">Array of parameters to send with the request. There must be at least 1 parameter.</param>
        /// <param name="TimeoutMilliseconds">The maximum number of milliseconds to wait for a response before timing out.</param>
        /// <param name="IsLongPolling">The maximum number of milliseconds to wait for a response before timing out.</param>
        public RequestMessage(object[] Parameters, int TimeoutMilliseconds, bool IsLongPolling = false) : base(MessageTypes.RequestMessage, TimeoutMilliseconds)
        {
            _parameters = Parameters;
            _isLongPolling = IsLongPolling;

            //  CREATE A REQUEST ID
            lock (_lockMaxRequestId)
            {
                if (_maxRequestId + 1 > long.MaxValue) _maxRequestId = 1;
                else _maxRequestId = _maxRequestId + 1;
                _requestId = _maxRequestId;
            }

            //  SERIALIZE REQUEST MESSAGE
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                writer.Write(_requestId);
                writer.Write(_isLongPolling);
                SerializeParameters(writer, Parameters);
                using (BinaryReader reader = new BinaryReader(writer.BaseStream))
                {
                    reader.BaseStream.Position = 0;
                    _parameterBytes = reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length));
                }
            }
        }


        internal RequestMessage(BinaryReader bR) : base(MessageTypes.RequestMessage)
        {
            _requestId = bR.ReadInt64();
            _isLongPolling = bR.ReadBoolean();
            _parameters = DeserializeParameters(bR);
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


        /// <summary>
        /// Identifier assigned to this request when it was created (usually when created at the client)
        /// </summary>
        public long RequestId
        {
            get { return _requestId; }
        }

        public ResponseMessage Response
        {
            get { lock (_lock) { return _response; } }
            set { lock (_lock) { _response = value; } }
        }

        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_parameterBytes);
        }
    }
}

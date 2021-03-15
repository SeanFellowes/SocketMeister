#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0063 // Use simple 'using' statement
#pragma warning disable CA1805 // Do not initialize unnecessarily

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketMeister.Messages
{
#if !SILVERLIGHT && !SMNOSERVER && !NET35 && !NET20
    internal partial class Message : MessageBase
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
    internal partial class Message : MessageBase, IMessage
    {
        //  REQUEST ID
        private static long _maxRequestId = 0;
        private static readonly object _lockMaxRequestId = new object();

        //  REQUEST VARIABLES
        private readonly bool _isLongPolling = false;
        private bool _isResponseReceived = false;
        private readonly byte[] _parameterBytes = null;
        private readonly object[] _parameters = null;
        private readonly long _requestId;
        private readonly DateTime _timeout;
        private readonly int _timeoutMilliseconds;

        //  INTERNAL (NOT SENT IN MESSAGE DATA)
        private MessageResponse _response = null;

        /// <summary>
        /// RequestMessage constructor
        /// </summary>
        /// <param name="Parameters">Array of parameters to send with the request. There must be at least 1 parameter.</param>
        /// <param name="TimeoutMilliseconds">The maximum number of milliseconds to wait for a response before timing out.</param>
        /// <param name="IsLongPolling">The maximum number of milliseconds to wait for a response before timing out.</param>
        public Message(object[] Parameters, int TimeoutMilliseconds, bool IsLongPolling = false) : base(MessageTypes.RequestMessageV1)
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


        internal Message(BinaryReader bR, int Version) : base(MessageTypes.RequestMessageV1)
        {
            if (Version == 1)
            {
                _requestId = bR.ReadInt64();
                _timeoutMilliseconds = 60000;       //  NOT IN VERSION 1
                _isLongPolling = bR.ReadBoolean();
                _parameters = Serializer.DeserializeParameters(bR);
            }
            else if (Version == 2)
            {
                _requestId = bR.ReadInt64();
                _timeoutMilliseconds = bR.ReadInt32();
                _isLongPolling = bR.ReadBoolean();
                _parameters = Serializer.DeserializeParameters(bR);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(Version), "Only versions 1 and 2 have a deserializer");
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

        public MessageResponse Response
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


#pragma warning restore CA1805 // Do not initialize unnecessarily
#pragma warning restore IDE0063 // Use simple 'using' statement
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression
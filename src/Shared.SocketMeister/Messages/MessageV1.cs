using System;
using System.IO;

namespace SocketMeister.Messages
{
#if !SMNOSERVER && !NET35
    internal partial class MessageV1 : MessageBase
    {
        private SocketServer.Client _remoteClient = null;

        /// <summary>
        /// The remote client which sent this Message (value null on SocketClient)
        /// </summary>
        internal SocketServer.Client RemoteClient
        {
            get { lock (Lock) { return _remoteClient; } }
            set { lock (Lock) { _remoteClient = value; } }
        }
    }
#endif

    /// <summary>
    /// A message, sent from socket client to socket server. A response is expected and will cause problems if it is not sent.
    /// </summary>
    internal partial class MessageV1 : MessageBase, IMessage
    {
        //  MESSAGE ID
        private static long _maxMessageId = 0;
        private static readonly object _lockMaxMessageId = new object();

        //  MESSAGE VARIABLES
        private readonly bool _isLongPolling = false;
        private readonly byte[] _parameterBytes = null;
        private readonly object[] _parameters = null;
        private readonly long _messageId;

        /// <summary>
        /// Message constructor
        /// </summary>
        /// <param name="Parameters">Array of parameters to send with the message. There must be at least 1 parameter.</param>
        /// <param name="TimeoutMilliseconds">The maximum number of milliseconds to wait for a response before timing out.</param>
        /// <param name="IsLongPolling">The maximum number of milliseconds to wait for a response before timing out.</param>
        public MessageV1(object[] Parameters, int TimeoutMilliseconds, bool IsLongPolling = false) : base(MessageType.MessageV1)
        {
            base.TimeoutMilliseconds = TimeoutMilliseconds;
            _parameters = Parameters;
            _isLongPolling = IsLongPolling;

            //  CREATE A MESSAGE ID
            lock (_lockMaxMessageId)
            {
                if (_maxMessageId + 1 > long.MaxValue) _maxMessageId = 1;
                else _maxMessageId += 1;
                _messageId = _maxMessageId;
            }

            //  SERIALIZE MESSAGE
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                writer.Write(_messageId);
                writer.Write(base.TimeoutMilliseconds);
                writer.Write(_isLongPolling);
                Serializer.SerializeParameters(writer, _parameters);
                using (BinaryReader reader = new BinaryReader(writer.BaseStream))
                {
                    reader.BaseStream.Position = 0;
                    _parameterBytes = reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length));
                }
            }
        }


        internal MessageV1(BinaryReader bR) : base(MessageType.MessageV1)
        {
            _messageId = bR.ReadInt64();
            base.TimeoutMilliseconds = bR.ReadInt32();
            _isLongPolling = bR.ReadBoolean();
            _parameters = Serializer.DeserializeParameters(bR);
        }


        /// <summary>
        /// Parameters provided with this message
        /// </summary>
        public object[] Parameters => _parameters;

        /// <summary>
        /// True is this message is long polling on the server side. Long polling messages will be closed immediately in the event of a close from either the client or server side.
        /// </summary>
        public bool IsLongPolling => _isLongPolling;

        /// <summary>
        /// Identifier assigned to this message when it was created
        /// </summary>
        public long MessageId => _messageId;


        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_parameterBytes);
        }


    }
}
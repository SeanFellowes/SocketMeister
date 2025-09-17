using System;
using System.Globalization;
using System.IO;

namespace SocketMeister.Messages
{
#if !NET35
    internal partial class MessageResponseV1 : MessageBase
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


    internal partial class MessageResponseV1 : MessageBase, IMessage
    {
        //  RESPONSE VARIABLES
        private readonly MessageEngineDeliveryResult _processingResult;
        private readonly byte[] _responseData = null;

        public MessageResponseV1(long MessageId, byte[] ResponseData) : base(MessageType.MessageResponseV1, MessageId, nameof(MessageResponseV1))
        {
            _responseData = ResponseData;
            _processingResult = MessageEngineDeliveryResult.Success;
        }

        public MessageResponseV1(long MessageId, MessageEngineDeliveryResult ProcessingResult) : base(MessageType.MessageResponseV1, MessageId, nameof(MessageResponseV1))
        {
            _processingResult = ProcessingResult;
        }

        public MessageResponseV1(long MessageId, Exception Exception) : base(MessageType.MessageResponseV1, MessageId, nameof(MessageResponseV1))
        {
            _processingResult = MessageEngineDeliveryResult.Exception;
        }

        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public MessageResponseV1(BinaryReader Reader) : base(MessageType.MessageResponseV1, Reader.ReadInt64(), nameof(MessageResponseV1))
        {
            if (Reader.ReadBoolean() == true) _responseData = Reader.ReadBytes(Reader.ReadInt32());
            if (Reader.ReadBoolean() == true)
            _processingResult = (MessageEngineDeliveryResult)Reader.ReadInt16();
        }

        public Byte[] ResponseData => _responseData;


        public MessageEngineDeliveryResult ProcessingResult => _processingResult;



        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(MessageId);
            if (_responseData == null) Writer.Write(false);
            else
            {
                Writer.Write(true);
                Writer.Write(_responseData.Length);
                Writer.Write(_responseData);
            }
            if (Error == null) Writer.Write(false);
            else
            {
                Writer.Write(true);
                Writer.Write(Error.Message);
            }
            Writer.Write(Convert.ToInt16(_processingResult, CultureInfo.InvariantCulture));
        }
    }
}

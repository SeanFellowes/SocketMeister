﻿#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1805 // Do not initialize unnecessarily

using System;
using System.Globalization;
using System.IO;

namespace SocketMeister.Messages
{
#if !SMNOSERVER && !NET35
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
        private readonly string _error = null;
        private readonly long _messageId;
        private readonly MessageEngineDeliveryResult _processingResult;
        private readonly byte[] _responseData = null;

        public MessageResponseV1(long MessageId, byte[] ResponseData) : base(MessageEngineMessageType.MessageResponseV1)
        {
            _messageId = MessageId;
            _responseData = ResponseData;
            _processingResult = MessageEngineDeliveryResult.Success;
        }

        public MessageResponseV1(long MessageId, MessageEngineDeliveryResult ProcessingResult) : base(MessageEngineMessageType.MessageResponseV1)
        {
            _messageId = MessageId;
            _processingResult = ProcessingResult;
        }

        public MessageResponseV1(long MessageId, Exception Exception) : base(MessageEngineMessageType.MessageResponseV1)
        {
            _messageId = MessageId;
            _processingResult = MessageEngineDeliveryResult.Exception;
            _error = Exception.Message;
            if (Exception.StackTrace != null) _error += Environment.NewLine + Environment.NewLine + Exception.StackTrace;
        }

        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public MessageResponseV1(BinaryReader Reader) : base(MessageEngineMessageType.MessageResponseV1)
        {
            _messageId = Reader.ReadInt64();
            if (Reader.ReadBoolean() == true) _responseData = Reader.ReadBytes(Reader.ReadInt32());
            if (Reader.ReadBoolean() == true) _error = Reader.ReadString();
            _processingResult = (MessageEngineDeliveryResult)Reader.ReadInt16();
        }

        /// <summary>
        /// The message that this response refers to
        /// </summary>
        public long MessageId => _messageId;


        public Byte[] ResponseData => _responseData;


        public MessageEngineDeliveryResult ProcessingResult => _processingResult;


        public string Error => _error;


        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_messageId);
            if (_responseData == null) Writer.Write(false);
            else
            {
                Writer.Write(true);
                Writer.Write(_responseData.Length);
                Writer.Write(_responseData);
            }
            if (_error == null) Writer.Write(false);
            else
            {
                Writer.Write(true);
                Writer.Write(_error);
            }
            Writer.Write(Convert.ToInt16(_processingResult, CultureInfo.InvariantCulture));
        }
    }
}

#pragma warning restore CA1805 // Do not initialize unnecessarily
#pragma warning restore IDE0079 // Remove unnecessary suppression

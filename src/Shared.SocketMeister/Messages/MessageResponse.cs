﻿#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1805 // Do not initialize unnecessarily

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SocketMeister.Messages
{
#if !SILVERLIGHT && !SMNOSERVER && !NET35 && !NET20
    internal partial class MessageResponse : MessageBase
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


    internal partial class MessageResponse : MessageBase, IMessage
    {
        //  RESPONSE VARIABLES
        private readonly string _error = null;
        private readonly long _messageId;
        private readonly RequestResult _requestResultCode;
        private readonly Byte[] _responseData = null;

        public MessageResponse(long MessageId, byte[] ResponseData) : base(MessageTypes.ResponseMessageV1)
        {
            _messageId = MessageId;
            _responseData = ResponseData;
            _requestResultCode = RequestResult.Success;
        }

        public MessageResponse(long MessageId, RequestResult RequestResultCode) : base(MessageTypes.ResponseMessageV1)
        {
            _messageId = MessageId;
            _requestResultCode = RequestResultCode;
        }

        public MessageResponse(long MessageId, Exception Exception) : base(MessageTypes.ResponseMessageV1)
        {
            _messageId = MessageId;
            _requestResultCode = RequestResult.Exception;
            _error = Exception.Message;
            if (Exception.StackTrace != null) _error += Environment.NewLine + Environment.NewLine + Exception.StackTrace;
        }

        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public MessageResponse(BinaryReader Reader) : base(MessageTypes.ResponseMessageV1)
        {
            _messageId = Reader.ReadInt64();
            if (Reader.ReadBoolean() == true) _responseData = Reader.ReadBytes(Reader.ReadInt32());
            if (Reader.ReadBoolean() == true) _error = Reader.ReadString();
            _requestResultCode = (RequestResult)Reader.ReadInt16();
        }

        /// <summary>
        /// The message that this response refers to
        /// </summary>
        public long MessageId
        {
            get { return _messageId; }
        }


        public Byte[] ResponseData
        {
            get { return _responseData; }
        }


        public RequestResult RequestResultCode
        {
            get { return _requestResultCode; }
        }


        public string Error
        {
            get { return _error; }
        }


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
            Writer.Write(Convert.ToInt16(_requestResultCode, CultureInfo.InvariantCulture));
        }
    }
}

#pragma warning restore CA1805 // Do not initialize unnecessarily
#pragma warning restore IDE0079 // Remove unnecessary suppression
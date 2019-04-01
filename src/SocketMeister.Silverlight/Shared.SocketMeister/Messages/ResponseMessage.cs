using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SocketMeister.Messages
{
    internal class ResponseMessage : MessageBase, IMessage
    {
        //  RESPONSE VARIABLES
        private readonly string _error = null;
        private readonly long _requestId;
        private readonly Byte[] _responseData = null;
        private readonly bool _serverIsStopping = false;

        public ResponseMessage(long RequestId, byte[] ResponseData) : base(MessageTypes.ResponseMessage)
        {
            _requestId = RequestId;
            _responseData = ResponseData;
        }

        public ResponseMessage(long RequestId, Exception Exception) : base(MessageTypes.ResponseMessage)
        {
            _requestId = RequestId;
            _error = Exception.Message;
            if (Exception.StackTrace != null) _error += Environment.NewLine + Environment.NewLine + Exception.StackTrace;
        }



        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public ResponseMessage(BinaryReader Reader) : base(MessageTypes.ResponseMessage)
        {
            _requestId = Reader.ReadInt64();
            if (Reader.ReadBoolean() == true) _responseData = Reader.ReadBytes(Reader.ReadInt32());
            if (Reader.ReadBoolean() == true) _error = Reader.ReadString();
            _serverIsStopping = Reader.ReadBoolean();
        }

        /// <summary>
        /// The request that this response refers to
        /// </summary>
        public long RequestId
        {
            get { return _requestId; }
        }


        public Byte[] ResponseData
        {
            get { return _responseData; }
        }


        public string Error
        {
            get { return _error; }
        }

        public bool ServerIsStopping
        {
            get { return _serverIsStopping; }
        }



        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_requestId);
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
            Writer.Write(_serverIsStopping);
        }
    }
}

using System;
using System.IO;

namespace SocketMeister.Messages
{
    internal class ResponseMessage : MessageBase, IMessage
    {
        //  RESPONSE VARIABLES
        private readonly string error = null;
        private readonly long requestId;
        private readonly Byte[] responseData = null;
        private readonly bool serverIsStopping = false;

        public ResponseMessage(long requestId, byte[] responseData) : base(MessageTypes.ResponseMessage)
        {
            this.requestId = requestId;
            this.responseData = responseData;
        }

        public ResponseMessage(long requestId, Exception exception) : base(MessageTypes.ResponseMessage)
        {
            this.requestId = requestId;
            error = exception.Message;
            if (exception.StackTrace != null) error += Environment.NewLine + Environment.NewLine + exception.StackTrace;
        }



        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="reader">Binary Reader</param>
        public ResponseMessage(BinaryReader reader) : base(MessageTypes.ResponseMessage)
        {
            requestId = reader.ReadInt64();
            if (reader.ReadBoolean() == true) responseData = reader.ReadBytes(reader.ReadInt32());
            if (reader.ReadBoolean() == true) error = reader.ReadString();
            serverIsStopping = reader.ReadBoolean();
        }

        /// <summary>
        /// The request that this response refers to
        /// </summary>
        public long RequestId
        {
            get { return requestId; }
        }


        public Byte[] ResponseData
        {
            get { return responseData; }
        }


        public string Error
        {
            get { return error; }
        }

        public bool ServerIsStopping
        {
            get { return serverIsStopping; }
        }



        public void AppendBytes(BinaryWriter writer)
        {
            writer.Write(requestId);
            if (responseData == null) writer.Write(false);
            else
            {
                writer.Write(true);
                writer.Write(responseData.Length);
                writer.Write(responseData);
            }
            if (error == null) writer.Write(false);
            else
            {
                writer.Write(true);
                writer.Write(error);
            }
            writer.Write(serverIsStopping);
        }
    }
}

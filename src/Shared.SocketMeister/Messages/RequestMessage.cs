using System;
using System.IO;


namespace SocketMeister.Messages
{
#if !SILVERLIGHT && !SMNOSERVER
    internal partial class RequestMessage
    {
        private SocketServer.Client remoteClient = null;

        /// <summary>
        /// The remote client which sent this RequestMessage (value null on SocketClient)
        /// </summary>
        internal SocketServer.Client RemoteClient
        {
            get { lock (classLock) { return remoteClient; } }
            set { lock (classLock) { remoteClient = value; } }
        }
    }
#endif

    /// <summary>
    /// A request, sent from socket client to socket server. A response is expected and will cause problems if it is not sent.
    /// </summary>
    internal partial class RequestMessage : MessageBase, IMessage
    {
        //  REQUEST ID
        private static long maxRequestId = 0;
        private static readonly object lockMaxRequestId = new object();

        //  REQUEST VARIABLES
        private readonly bool isLongPolling = false;
        private readonly byte[] parameterBytes = null;
        private readonly object[] parameters = null;
        private readonly long requestId;

        //  INTERNAL (NOT SENT IN MESSAGE DATA)
        private readonly object classLock = new object();
        bool disposed = false;
        private ResponseMessage response = null;


        /// <summary>
        /// RequestMessage constructor
        /// </summary>
        /// <param name="parameters">Array of parameters to send with the request. There must be at least 1 parameter.</param>
        /// <param name="timeoutMilliseconds">The maximum number of milliseconds to wait for a response before timing out.</param>
        /// <param name="isLongPolling">The maximum number of milliseconds to wait for a response before timing out.</param>
        public RequestMessage(object[] parameters, int timeoutMilliseconds, bool isLongPolling = false) : base(MessageTypes.RequestMessage, timeoutMilliseconds)
        {
            this.parameters = parameters;
            this.isLongPolling = isLongPolling;

            //  CREATE A REQUEST ID
            lock (lockMaxRequestId)
            {
                if (maxRequestId + 1 > long.MaxValue) maxRequestId = 1;
                else maxRequestId += 1;
                requestId = maxRequestId;
            }

            //  SERIALIZE REQUEST MESSAGE
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
            {
                writer.Write(requestId);
                writer.Write(this.isLongPolling);
                SerializeParameters(writer, parameters);
                using (BinaryReader reader = new BinaryReader(writer.BaseStream))
                {
                    reader.BaseStream.Position = 0;
                    parameterBytes = reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length));
                }
            }
        }


        internal RequestMessage(BinaryReader reader) : base(MessageTypes.RequestMessage)
        {
            requestId = reader.ReadInt64();
            isLongPolling = reader.ReadBoolean();
            parameters = DeserializeParameters(reader);
        }

        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (response != null) response.Dispose();
            }

            disposed = true;
            // Call base class implementation.
            base.Dispose(disposing);
        }



        /// <summary>
        /// Parameters provided with this request
        /// </summary>
        public object[] Parameters
        {
            get { return parameters; }
        }

        /// <summary>
        /// True is this request is long polling on the server side. Long polling requests will be closed immediately in the event of a close from either the client or server side.
        /// </summary>
        public bool IsLongPolling
        {
            get { return isLongPolling; }
        }


        /// <summary>
        /// Identifier assigned to this request when it was created (usually when created at the client)
        /// </summary>
        public long RequestId
        {
            get { return requestId; }
        }

        public ResponseMessage Response
        {
            get { lock (classLock) { return response; } }
            set { lock (classLock) { response = value; } }
        }

        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(parameterBytes);
        }
    }
}

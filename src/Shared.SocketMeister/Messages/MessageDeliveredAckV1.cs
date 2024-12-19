using System;
using System.Globalization;
using System.IO;

namespace SocketMeister.Messages
{
#if !SMNOSERVER && !NET35
    internal partial class MessageDeliveredAckV1 : MessageBase
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
    /// Light message sent by the message receiver to acknowledge receipt. Some system messages do not send an ack.
    /// </summary>
    internal partial class MessageDeliveredAckV1 : MessageBase, IMessage
    {
        //  RESPONSE VARIABLES
        private readonly long _messageId;

        public MessageDeliveredAckV1(long MessageId) : base(MessageType.MessageDeliveredAckV1, waitForResponse: false)
        {
            _messageId = MessageId;
        }

        /// <summary>
        /// Fastest was to build this is to create it directly from the SocketEnvelope buffer.
        /// </summary>
        /// <param name="Reader">Binary Reader</param>
        public MessageDeliveredAckV1(BinaryReader Reader) : base(MessageType.MessageDeliveredAckV1, waitForResponse: false)
        {
            _messageId = Reader.ReadInt64();
        }

        /// <summary>
        /// The message that this response refers to
        /// </summary>
        public long MessageId => _messageId;


        public void AppendBytes(BinaryWriter Writer)
        {
            Writer.Write(_messageId);
        }
    }
}
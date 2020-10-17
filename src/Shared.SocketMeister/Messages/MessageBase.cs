using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace SocketMeister.Messages
{
    internal partial class MessageBase
    {
        private bool _isAborted;
        private readonly object _lock = new object();
        private readonly MessageTypes _messageType;
        private MessageStatus _messageStatus = MessageStatus.Unsent;
        private readonly int _serializerVersion;    


        internal MessageBase(MessageTypes MessageType, int SerializationVersion)
        {
            _messageType = MessageType;
            _serializerVersion = SerializationVersion;
        }


        public object Lock { get { return _lock; } }

        public bool IsAborted
        {
            get { lock (_lock) { return _isAborted; } }
            set { lock (_lock) { _isAborted = value; } }
        }

        public MessageTypes MessageType { get { return _messageType; } }

        /// <summary>
        /// Version of the serialized message. This provides the ability add new functionality to each message type, but maintain, for as long as possible, backward compatibility.
        /// </summary>
        public int SerializerVersion
        {
            get { lock (_lock) { return _serializerVersion; } }
        }


        public MessageStatus Status
        {
            get { lock (_lock) { return _messageStatus; } }
            set { lock (_lock) { _messageStatus = value; } }
        }


    }
}

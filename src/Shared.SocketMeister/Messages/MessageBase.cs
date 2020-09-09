using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace SocketMeister.Messages
{
    internal partial class MessageBase
    {
        private bool _isAborted = false;
        private readonly object _lock = new object();
        private readonly MessageTypes _messageType;
        private MessageStatus _messageStatus = MessageStatus.Unsent;

        internal MessageBase(MessageTypes MessageType)
        {
            _messageType = MessageType;
        }

        public object Lock {  get { return _lock; } }

        public bool IsAborted
        {
            get { lock (_lock) { return _isAborted; } }
            set { lock (_lock) { _isAborted = value; } }
        }


        public MessageTypes MessageType { get { return _messageType; } }


        public MessageStatus Status
        {
            get { lock (_lock) { return _messageStatus; } }
            set { lock (_lock) { _messageStatus = value; } }
        }


    }
}

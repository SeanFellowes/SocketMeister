#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0090 // Use 'new(...)'

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
        private readonly MessageEngineMessageType _messageType;
        private MessageEngineDeliveryStatus _messageStatus = MessageEngineDeliveryStatus.Unsent;

        internal MessageBase(MessageEngineMessageType MessageType)
        {
            _messageType = MessageType;
        }

        public object Lock { get { return _lock; } }

        public bool IsAborted
        {
            get { lock (_lock) { return _isAborted; } }
            set { lock (_lock) { _isAborted = value; } }
        }


        public MessageEngineMessageType MessageType { get { return _messageType; } }


        public MessageEngineDeliveryStatus Status
        {
            get { lock (_lock) { return _messageStatus; } }
            set { lock (_lock) { _messageStatus = value; } }
        }

    }
}

#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression

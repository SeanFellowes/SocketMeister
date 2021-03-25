#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0090 // Use 'new(...)'


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

        public object Lock => _lock;

        public bool IsAborted
        {
            get { lock (_lock) { return _isAborted; } }
            set { lock (_lock) { _isAborted = value; } }
        }


        public MessageEngineMessageType MessageType => _messageType;


        public MessageEngineDeliveryStatus Status
        {
            get { lock (_lock) { return _messageStatus; } }
            set { lock (_lock) { _messageStatus = value; } }
        }

    }
}

#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression

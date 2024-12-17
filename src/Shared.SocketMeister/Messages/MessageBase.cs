#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0090 // Use 'new(...)'

#if !NET35
using System;
using System.Threading;
#endif


namespace SocketMeister.Messages
{
#if !NET35
    internal partial class MessageBase : IDisposable
#else
    internal partial class MessageBase
#endif
    {
        private bool _isAborted;
        private readonly object _lock = new object();
        private readonly MessageEngineMessageType _messageType;
        private MessageEngineDeliveryStatus _messageStatus = MessageEngineDeliveryStatus.Unsent;
#if !NET35
        internal ManualResetEventSlim ResponseReceivedEvent { get; set; }
#endif


        internal MessageBase(MessageEngineMessageType MessageType)
        {
            _messageType = MessageType;
        }


#if !NET35
        /// <summary>
        /// Dispose of the class
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the class
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            //  Note. THis is incomplete. Ask ChatGPT to help look for missing things
            if (disposing)
            {
                ResponseReceivedEvent?.Dispose();
            }
        }
#endif


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
            set 
            { 
                lock (_lock) 
                { 
                    _messageStatus = value;
#if !NET35
                    if (value ==  MessageEngineDeliveryStatus.ResponseReceived) ResponseReceivedEvent?.Set();
#endif

                }
            }
        }

    }
}

#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression

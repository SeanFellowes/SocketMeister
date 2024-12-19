using System;
#if !NET35
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
        private readonly MessageType _messageType;
        private MessageStatus _messageStatus = MessageStatus.Unsent;

        //  After sending this message, wait for a response.
        public bool WaitForResponse { get; }

        public DateTime CreatedDateTime { get;}

        public IMessage Response { get; set; }


#if !NET35
        //  This is only used by some message types, so is null by default
        public ManualResetEventSlim ResponseReceivedEvent { get; set; }
#endif


        internal MessageBase(MessageType MessageType, bool waitForResponse)
        {
            _messageType = MessageType;
            CreatedDateTime = DateTime.UtcNow;
            WaitForResponse = waitForResponse;
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
        }

        /// <summary>
        /// Abort the message (For example socket closed or SocketServer/SocketClient is stopping or disposing)
        /// </summary>
        public void Abort()
        {
            lock (_lock) 
            { 
                _isAborted = true; 
            } 
        }


        public MessageType MessageType => _messageType;


        public MessageStatus Status
        {
            get { lock (_lock) { return _messageStatus; } }
            set 
            { 
                lock (_lock) 
                { 
                    _messageStatus = value;
#if !NET35
                    if (value == MessageStatus.Completed) ResponseReceivedEvent?.Set();
#endif

                }
            }
        }

    }
}

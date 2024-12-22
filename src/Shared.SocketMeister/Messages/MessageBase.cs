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
        private Exception _error;
        private bool _isAborted;
        private readonly object _lock = new object();
        private readonly MessageType _messageType;
        private MessageStatus _status = MessageStatus.Unsent;
        private MessageResponseV1 _response;
        private int _timeoutMilliseconds = 60000;
        private DateTime _timeoutDateTime;
#if !NET35
        private ManualResetEventSlim _responseReceivedEvent;
#endif

        /// <summary>
        /// Event raised when the current EndPoint channges
        /// </summary>
        public event EventHandler<EventArgs> SendReceiveStatusChanged;


        internal MessageBase(MessageType MessageType)
        {
            _messageType = MessageType;
            CreatedDateTime = DateTime.UtcNow;
            _timeoutDateTime = DateTime.UtcNow.AddMilliseconds(_timeoutMilliseconds);
        }


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
#if !NET35
                _responseReceivedEvent?.Dispose();
#endif
                _response?.Dispose();
            }
        }

        /// <summary>
        /// Timestamp, in UTC, when the message was created (Set during message instantiation)
        /// </summary>
        public DateTime CreatedDateTime { get; }

        /// <summary>
        /// Error which occured when the message failed
        /// </summary>
        public Exception Error
        {
            get { lock (_lock) { return _error; } }
        }

        /// <summary>
        /// Number of milliseconds until the message will timeout. Also sets TimeoutDateTime
        /// </summary>
        public int TimeoutMilliseconds
        {
            get
            {
                lock (_lock) { return _timeoutMilliseconds; }
            }
            set
            {
                lock (_lock)
                {
                    _timeoutMilliseconds = value;
                    _timeoutDateTime = DateTime.UtcNow.AddMilliseconds(_timeoutMilliseconds);
                }
            }
        }

        public object Lock => _lock;

        /// <summary>
        /// Message is aborted, usually due to a SocketServer or SocketClient shutdown or disposal().
        /// </summary>
        public bool IsAborted
        {
            get { lock (_lock) { return _isAborted; } }
            set { lock (_lock) { _isAborted = value; } }
        }


        /// <summary>
        /// Whether the Message has timed out
        /// </summary>
        public bool IsTimeout
        {
            get
            {
                if (DateTime.UtcNow > _timeoutDateTime) return true;
                else return false;
            }
        }

        /// <summary>
        /// Type of message
        /// </summary>
        public MessageType MessageType => _messageType;


        /// <summary>
        /// Response message, where a response is required (waitForResponse = true)
        /// </summary>
        public MessageResponseV1 Response
        {
            get { lock (_lock) { return _response; } }
        }

        public void SetStatusCompleted()
        {
            SetStatus(MessageStatus.Completed);
        }

        public void SetStatusCompleted(MessageResponseV1 responseMessage)
        {
            SetStatus(MessageStatus.Completed, responseMessage);
        }

        public void SetStatusCompleted(Exception exception)
        {
            SetStatus(MessageStatus.Completed, null, exception);
        }


        public void SetStatusInProgress()
        {
            SetStatus(MessageStatus.InProgress);
        }

        public void SetStatusUnsent()
        {
            if (SendReceiveStatus != MessageStatus.InProgress) return;
            SetStatus(MessageStatus.Unsent);
        }

        private void SetStatus(MessageStatus value, MessageResponseV1 responseMessage = null, Exception exception = null)
        {
            bool changed = false;
            lock (_lock)
            {
                if (exception != null) 
                    _error = exception;

                if (responseMessage != null) 
                    _response = responseMessage;

                if (value != _status)
                {
                    changed = true;
                    _status = value;
                }

#if !NET35
                if (_status == MessageStatus.Completed)
                {
                    _responseReceivedEvent?.Set();
                    _responseReceivedEvent?.Dispose();
                }
#endif



            }
            if (changed) SendReceiveStatusChanged?.Invoke(this, new EventArgs());
        }


        /// <summary>
        /// Calculated SendReceive status of the message (Will consider timeout accurately)
        /// </summary>
        public MessageStatus SendReceiveStatus
        {
            get
            {
                bool incompleteMessageTimedOut;

                lock (_lock)
                {
                    if (_status == MessageStatus.Completed) return _status;
                    //  Status must be Unsent or In Progress. Check for timeout
                    if (DateTime.UtcNow > _timeoutDateTime) incompleteMessageTimedOut = true;
                    else return _status;
                }
                //  The message is in progress and has timed out
                if (incompleteMessageTimedOut) SetStatusCompleted(new TimeoutException());
                return MessageStatus.Completed;
            }
        }


        /// <summary>
        /// Whether the method SendReceive(Message Message) should continute trying
        /// </summary>
        public bool TrySendReceive
        {
            get
            {
                if (SendReceiveStatus == MessageStatus.Completed) return false;
                return true;
            }
        }


#if !NET35
        /// <summary>
        ///     Waits for SetCompleted() has been called or the message times out.
        /// </summary>
        /// <returns></returns>
        public bool WaitForCompleted()
        {
            int remainingMilliseconds;
            lock (_lock) 
            {
                _responseReceivedEvent = new ManualResetEventSlim(false); 
                remainingMilliseconds = _timeoutMilliseconds - (int)(DateTime.UtcNow - CreatedDateTime).TotalMilliseconds;
            }
            if (remainingMilliseconds > 0)
            {
                return _responseReceivedEvent.Wait(remainingMilliseconds);
            }
            return false;
        }
#endif
}
}

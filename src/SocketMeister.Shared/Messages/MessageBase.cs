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
        //  MESSAGE ID
        private static long _maxMessageId = 0;
        private static readonly object _lockMaxMessageId = new object();

        private Exception _error;
        private string _friendlyMessageName = string.Empty;
        private readonly object _lock = new object();
        private readonly long _messageId;
        private readonly MessageType _messageType;
        private SendStatus _status = SendStatus.Unsent;
        private MessageResponseV1 _response;
        private int _sendTimeoutMs = 60000;
        private DateTime _responseTimeoutUtc;
#if !NET35
        private readonly ManualResetEventSlim _sendCompletionEvent = new ManualResetEventSlim(false);
#endif

        /// <summary>
        /// Event raised when the current EndPoint channges
        /// </summary>
        public event EventHandler<EventArgs> SendReceiveStatusChanged;


        internal MessageBase(MessageType MessageType, long messageId, string friendlyMessageName)
        {
            _messageType = MessageType;
            _friendlyMessageName = friendlyMessageName;
            CreatedDateTime = DateTime.UtcNow;
            _responseTimeoutUtc = DateTime.UtcNow.AddMilliseconds(_sendTimeoutMs);
            if (messageId == 0)
            {
                //  Create a MessageId
                lock (_lockMaxMessageId)
                {
                    if (_maxMessageId + 1 > long.MaxValue) _maxMessageId = 1;
                    else _maxMessageId += 1;
                    _messageId = _maxMessageId;
                }
            }
            else
            { _messageId = messageId; }
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
                _sendCompletionEvent?.Dispose();
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
        /// Optional friendly name for the message, used for logging and debugging purposes.
        /// </summary>
        public string FriendlyMessageName
        {
            get { lock (_lock) { return _friendlyMessageName; } }
        }

        public long MessageId
        {
            get { return _messageId; }
        }

        /// <summary>
        /// Number of milliseconds until the message will timeout. Also sets TimeoutDateTime
        /// </summary>
        public int TimeoutMilliseconds
        {
            get
            {
                lock (_lock) { return _sendTimeoutMs; }
            }
            set
            {
                lock (_lock)
                {
                    _sendTimeoutMs = value;
                    _responseTimeoutUtc = DateTime.UtcNow.AddMilliseconds(_sendTimeoutMs);
                }
            }
        }

        public object Lock => _lock;

        /// <summary>
        /// Whether the Message has timed out
        /// </summary>
        public bool IsTimeout
        {
            get
            {
                if (DateTime.UtcNow > _responseTimeoutUtc) return true;
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

        public void SetStatusCompleted(MessageResponseV1 responseMessage)
        {
            SetStatus(SendStatus.Completed, responseMessage);
        }

        public void SetStatusCompleted(Exception exception)
        {
            SetStatus(SendStatus.Completed, null, exception);
        }


        public void SetStatusInProgress()
        {
            SetStatus(SendStatus.InProgress);
        }

        public void SetStatusUnsent()
        {
            if (SendReceiveStatus != SendStatus.InProgress) return;
            SetStatus(SendStatus.Unsent);
        }

        private void SetStatus(SendStatus value, MessageResponseV1 responseMessage = null, Exception exception = null)
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

                    // Handle the send completion event
#if !NET35
                    if (_status == SendStatus.InProgress)
                    {
                        // Reset the event for a new send attempt (e.g., retry after socket failure)
                        _sendCompletionEvent.Reset();
                    }
                    else
                    {
                        // Signal that the send has completed (either success or failure)
                        _sendCompletionEvent.Set();
                    }
#endif
                }
            }
            if (changed) SendReceiveStatusChanged?.Invoke(this, new EventArgs());
        }


        /// <summary>
        /// Calculated SendReceive status of the message (Will consider timeout accurately)
        /// </summary>
        public SendStatus SendReceiveStatus
        {
            get
            {
                bool incompleteMessageTimedOut;

                lock (_lock)
                {
                    if (_status == SendStatus.Completed) return _status;
                    //  Status must be Unsent or In Progress. Check for timeout
                    if (DateTime.UtcNow > _responseTimeoutUtc) incompleteMessageTimedOut = true;
                    else return _status;
                }
                //  The message is in progress and has timed out
                if (incompleteMessageTimedOut) SetStatusCompleted(new TimeoutException());
                return SendStatus.Completed;
            }
        }


        /// <summary>
        /// Whether the method SendReceive(Message Message) should continute trying
        /// </summary>
        public bool TrySendReceive
        {
            get
            {
                if (SendReceiveStatus == SendStatus.Completed ) return false;
                if (IsTimeout) return false;
                return true;
            }
        }


#if !NET35
        /// <summary>
        /// Using a blocking wait, wait for the block to be set or timeout
        /// </summary>
        /// <exception cref="TimeoutException">The send operation timed out</exception>
        public void WaitForSendAttemptCompletion(int SendTimeoutMs)
        {
            if (_sendCompletionEvent.Wait(SendTimeoutMs) == false)
                throw new TimeoutException($"SendMessage() received no response out after {SendTimeoutMs} milliseconds.");
        }
#endif
    }
}

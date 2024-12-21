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
        private SendReceiveStatus _sendReceiveStatus = SendReceiveStatus.Unsent;
        private MessageResponseV1 _response;
        private int _timeoutMilliseconds = 60000;
        private DateTime _timeoutDateTime;
        //private readonly bool _waitForResponse;
#if !NET35
        private ManualResetEventSlim _responseReceivedEvent;
#endif

        /// <summary>
        /// Event raised when the current EndPoint channges
        /// </summary>
        public event EventHandler<EventArgs> SendReceiveStatusChanged;


        internal MessageBase(MessageType MessageType, bool waitForResponse)
        {
            _messageType = MessageType;
            CreatedDateTime = DateTime.UtcNow;
            //_waitForResponse = waitForResponse;
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

        public void SetToCompleted(MessageResponseV1 responseMessage)
        {
            SetSendReceiveStatus(SendReceiveStatus.Completed, responseMessage);
        }

        public void SetToInProgress()
        {
            SetSendReceiveStatus(SendReceiveStatus.InProgress);
        }

        public void SetToUnsent()
        {
            if (SendReceiveStatus != SendReceiveStatus.InProgress) return;
            SetSendReceiveStatus(SendReceiveStatus.Unsent);
        }

        private void SetSendReceiveStatus(SendReceiveStatus value, MessageResponseV1 responseMessage = null)
        {
            bool changed = false;
            lock (_lock)
            {   
                if (responseMessage != null)
                {
                    if (value != SendReceiveStatus.Completed) throw new ArgumentException("SendReceiveStatus must be " + nameof(SendReceiveStatus.Completed) + " when providing a response message", "value");
                    _response = responseMessage;
#if !NET35
                    _responseReceivedEvent?.Set();
                    _responseReceivedEvent?.Dispose();
#endif
                }

                if (value != _sendReceiveStatus)
                {
                    changed = true;
                    _sendReceiveStatus = value;
                }

            }
            if (changed) SendReceiveStatusChanged?.Invoke(this, new EventArgs());
        }


        /// <summary>
        /// Calculated SendReceive status of the message (Will consider timeout accurately)
        /// </summary>
        public SendReceiveStatus SendReceiveStatus
        {
            get
            {
                SendReceiveStatus calculatedStatus;

                lock (_lock)
                {
                    if (_sendReceiveStatus == SendReceiveStatus.Completed) calculatedStatus = SendReceiveStatus.Completed;
                    else if (DateTime.UtcNow > _timeoutDateTime) calculatedStatus = SendReceiveStatus.Timeout;
                    else calculatedStatus = _sendReceiveStatus;
                }
                SetSendReceiveStatus(calculatedStatus);
                return calculatedStatus;
            }
        }


        /// <summary>
        /// Whether the method SendReceive(Message Message) should continute trying
        /// </summary>
        public bool TrySendReceive
        {
            get
            {
                SendReceiveStatus currentStatus = SendReceiveStatus;
                if (currentStatus == SendReceiveStatus.Completed || currentStatus == SendReceiveStatus.Timeout) return false;
                return true;
            }
        }


#if !NET35
        /// <summary>
        ///     Waits for SetCompleted() has been called or the message times out.
        /// </summary>
        /// <returns></returns>
        public bool WaitForResponse()
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

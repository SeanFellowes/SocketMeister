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
        private SendReceiveStatus _messageStatus = SendReceiveStatus.Unsent;
        private MessageResponseV1 _response;
        private int _timeoutMilliseconds = 60000;
        private DateTime _timeoutDateTime;
        //private readonly bool _waitForResponse;
#if !NET35
        private ManualResetEventSlim _responseReceivedEvent;
#endif


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

        public void SetCompleted(MessageResponseV1 responseMessage)
        {
            lock (_lock)
            {
                _response = responseMessage;
                _messageStatus = SendReceiveStatus.Completed;
#if !NET35
                _responseReceivedEvent?.Set();
                _responseReceivedEvent?.Dispose();
#endif
            }
        }

        public void SetInProgress()
        {
            lock (_lock)
            {
                _messageStatus = SendReceiveStatus.InProgress;
            }
        }


        public void TryRetrySend()
        {
            if (Status != SendReceiveStatus.InProgress) return;
            _messageStatus = SendReceiveStatus.Unsent;
        }




        /// <summary>
        /// Calculated status of the message.
        /// </summary>
        public SendReceiveStatus Status
        {
            get
            {
                SendReceiveStatus calculatedStatus;
                bool statusChanged = false;

                lock (_lock)
                {
                    if (_messageStatus == SendReceiveStatus.Completed) calculatedStatus = SendReceiveStatus.Completed;
                    else if (DateTime.UtcNow > _timeoutDateTime) calculatedStatus = SendReceiveStatus.Timeout;
                    else calculatedStatus = _messageStatus;

                    if (calculatedStatus != _messageStatus)
                    {
                        statusChanged = true;
                        _messageStatus = calculatedStatus;
                    }
                }

                if (statusChanged)
                {
                    //  RAISE STATUS CHANGED EVENT
                }

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
                SendReceiveStatus currentStatus = Status;
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

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
        private MessageResponseV1 _response;
        private int _timeoutMilliseconds = 60000;
        private DateTime _timeoutDateTime;
        private readonly bool _waitForResponse;


#if !NET35
        //  This is only used by some message types, so is null by default
        public ManualResetEventSlim ResponseReceivedEvent { get; set; }
#endif


        internal MessageBase(MessageType MessageType, bool waitForResponse)
        {
            _messageType = MessageType;
            CreatedDateTime = DateTime.UtcNow;
            _waitForResponse = waitForResponse;
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
                ResponseReceivedEvent?.Dispose();
#endif
                _response?.Dispose();
            }
        }

        /// <summary>
        /// Timestamp, in UTC, when the message was created (Set during message instantiation)
        /// </summary>
        public DateTime CreatedDateTime { get; }

        ///// <summary>
        ///// Timestamp, in UTC, when the message will timeout.
        ///// </summary>
        //public DateTime TimeoutDateTime 
        //{ 
        //    get { lock(_lock) { return _timeoutDateTime; } }
        //}

        /// <summary>
        /// Number of milliseconds until the message will timeout. Also sets TimeoutDateTime
        /// </summary>
        public int TimeoutMilliseconds
        {
            get
            {
                lock (_lock)
                {
                    return _timeoutMilliseconds;
                }
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
                _messageStatus = MessageStatus.Completed;
#if !NET35
                ResponseReceivedEvent?.Set();
#endif

            }
        }

        public void SetInProgress()
        {
            lock (_lock)
            {
                _messageStatus = MessageStatus.InProgress;
            }
        }


        public void TryRetrySend()
        {
            lock (_lock)
            {
                _messageStatus = MessageStatus.Unsent;
            }
        }


        ////  Make sure this is called within a lock
        //private bool RetryPossible()
        //{
        //    if (_messageStatus == MessageStatus.Completed || _messageStatus == MessageStatus.
        //}



        /// <summary>
        /// Calculated status of the message.
        /// </summary>
        public MessageStatus Status
        {
            get
            {
                MessageStatus calculatedStatus;
                bool statusChanged = false;

                lock (_lock)
                {
                    if (_messageStatus == MessageStatus.Completed) calculatedStatus = MessageStatus.Completed;
                    else if (_messageStatus == MessageStatus.Aborted) calculatedStatus = MessageStatus.Aborted;
                    else if (DateTime.UtcNow > _timeoutDateTime) calculatedStatus = MessageStatus.Timeout;
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
        /// Whether a SendReceive process should continue waiting for a response
        /// </summary>
        public bool ContinueWaitingtForResponse
        {
            get
            {
                lock (_lock)
                {
                    if (!_waitForResponse) return false;
                    if (_isAborted) return false;
                    if (DateTime.UtcNow > _timeoutDateTime) return false;
                    if (_response != null) return false;
                    if (Status == MessageStatus.InProgress) return true;
                    return false;
                }
            }
        }

#if !NET35
        public bool WaitForResponse(ManualResetEventSlim messageWaitHandle)
        {
            int remainingMilliseconds;
            lock (_lock) 
            { 
                ResponseReceivedEvent = messageWaitHandle; 
                remainingMilliseconds = _timeoutMilliseconds - (int)(DateTime.UtcNow - CreatedDateTime).TotalMilliseconds;
            }
            if (remainingMilliseconds > 0)
            {
                return ResponseReceivedEvent.Wait(remainingMilliseconds);
            }
            return false;
        }
#endif
}
}

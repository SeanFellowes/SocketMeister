using SocketMeister.Messages;
using System.Collections.Generic;
using System.Threading;

namespace SocketMeister
{
    internal class UnrespondedMessageCollection
    {
        private readonly Dictionary<long, IMessage> _messages = new Dictionary<long, IMessage>();
#if !NET35
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
#else
        private readonly object _lock = new object();
#endif

        /// <summary>
        /// Safely retrieves a message if it exists.
        /// </summary>
        internal bool TryGetMessage(long RequestID, out IMessage message)
        {
#if !NET35
            _lock.EnterReadLock();
            try
            {
                return _messages.TryGetValue(RequestID, out message);
            }
            finally
            {
                _lock.ExitReadLock();
            }
#else
            lock (_lock)
            {
                return _messages.TryGetValue(RequestID, out message);
            }
#endif
        }


        /// <summary>
        /// Clears the collection. Called during parent Dispose
        /// </summary>
        internal void Clear()
        {
#if !NET35
            _lock.EnterWriteLock();
            try
            {
                _messages.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
#else
            lock (_lock)
            {
                _messages.Clear();
            }
#endif
        }


        /// <summary>
        /// Adds a new message to the collection.
        /// </summary>
        internal void Add(MessageV1 AddItem)
        {
#if !NET35
            _lock.EnterWriteLock();
            try
            {
                _messages.Add(AddItem.MessageId, AddItem);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
#else
            lock (_lock)
            {
                _messages.Add(AddItem.MessageId, AddItem);
            }
#endif
        }

        /// <summary>
        /// Removes a message from the collection.
        /// </summary>
        internal void Remove(MessageV1 RemoveItem)
        {
#if !NET35
            _lock.EnterWriteLock();
            try
            {
                _messages.Remove(RemoveItem.MessageId);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
#else
            lock (_lock)
            {
                _messages.Remove(RemoveItem.MessageId);
            }
#endif
        }

        /// <summary>
        /// Gets the total count of messages.
        /// </summary>
        public int Count
        {
            get
            {
#if !NET35
                _lock.EnterReadLock();
                try
                {
                    return _messages.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
#else
                lock (_lock)
                {
                    return _messages.Count;
                }
#endif
            }
        }

        /// <summary>
        /// Resets messages to 'Unsent' status.
        /// </summary>
        internal void ResetToUnsent()
        {
            List<IMessage> messagesCopy;

#if !NET35
            _lock.EnterReadLock();
            try
            {
                messagesCopy = new List<IMessage>(_messages.Values);
            }
            finally
            {
                _lock.ExitReadLock();
            }
#else
            lock (_lock)
            {
                messagesCopy = new List<IMessage>(_messages.Values);
            }
#endif

            // Process outside the lock
            foreach (var message in messagesCopy)
            {
                message.TryRetrySend();
            }
        }


        /// <summary>
        /// Finds a original message from the MessageId included in a response and sets the ResponseMessage to the original message to the ResponseMessage 
        /// </summary>
        /// <param name="ResponseMessage"></param>
        /// <returns></returns>
        internal bool FindMessageAndSetResponse(MessageResponseV1 ResponseMessage)
        {
            if (!TryGetMessage(ResponseMessage.MessageId, out var message))
                return false;

            // Update response outside the lock
            message.SetCompleted(ResponseMessage);
            return true;
        }
    }
}

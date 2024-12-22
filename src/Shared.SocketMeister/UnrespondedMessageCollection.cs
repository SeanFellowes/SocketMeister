using SocketMeister.Messages;
using System.Collections.Generic;
using System.Threading;

namespace SocketMeister
{
    /// <summary>
    /// Messages waiting for a response
    /// </summary>
    internal class UnrespondedMessageCollection
    {
        private readonly Dictionary<long, IMessage> _messages = new Dictionary<long, IMessage>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Clears the collection. Called during parent Dispose
        /// </summary>
        internal void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _messages.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }


        /// <summary>
        /// Adds a new message to the collection.
        /// </summary>
        internal void Add(MessageV1 AddItem)
        {
            _lock.EnterWriteLock();
            try
            {
                _messages.Add(AddItem.MessageId, AddItem);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes a message from the collection.
        /// </summary>
        internal void Remove(MessageV1 RemoveItem)
        {
            _lock.EnterWriteLock();
            try
            {
                _messages.Remove(RemoveItem.MessageId);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// After a disconnect, reset applicable messages to Unsent, so the messages can be resent if the client reconnects to the server
        /// </summary>
        internal void ResetAfterDisconnect()
        {
            List<IMessage> messagesCopy;

            _lock.EnterReadLock();
            try
            {
                messagesCopy = new List<IMessage>(_messages.Values);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            // Process outside the lock
            foreach (var message in messagesCopy)
            {
                message.SetStatusUnsent();
            }
        }


        /// <summary>
        /// Safely finds the original message in this class using the MessageId included in a response. If found it sets the ResponseMessage on the original message
        /// </summary>
        /// <param name="ResponseMessage"></param>
        /// <returns>true is successful</returns>
        internal bool FindMessageAndSetResponse(MessageResponseV1 ResponseMessage)
        {
            _lock.EnterReadLock();
            IMessage message;
            try
            {
                _messages.TryGetValue(ResponseMessage.MessageId, out message);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            // Update response outside the lock
            if (message != null)
            {
                message.SetStatusCompleted(ResponseMessage);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

using SocketMeister.Messages;
using System.Collections.Generic;
using System.Threading;

namespace SocketMeister
{
    /// <summary>
    /// A collection of messages that are waiting for a response.
    /// </summary>
    internal class UnrespondedMessageCollection
    {
        private readonly Dictionary<long, IMessage> _messages = new Dictionary<long, IMessage>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Clears all messages from the collection. This is typically called during the disposal of the parent object.
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
        /// <param name="AddItem">The message to add.</param>
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
        /// <param name="RemoveItem">The message to remove.</param>
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
        /// Resets applicable messages to the "Unsent" status after a disconnect, allowing them to be resent if the client reconnects to the server.
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

            // Process messages outside the lock.
            foreach (var message in messagesCopy)
            {
                message.SetStatusUnsent();
            }
        }

        /// <summary>
        /// Finds the original message in the collection using the MessageId from a response. 
        /// If found, sets the response message on the original message.
        /// </summary>
        /// <param name="ResponseMessage">The response message to associate with the original message.</param>
        /// <returns>The message which the response relates to from the unresponded messages list.</returns>
        internal IMessage FindMessageAndSetResponse(MessageResponseV1 ResponseMessage)
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

            // Update the response outside the lock.
            if (message != null)
            {
                message.SetStatusCompleted(ResponseMessage);
            }
            return message;
        }
    }
}

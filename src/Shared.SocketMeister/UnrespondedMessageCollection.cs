#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0090 // Use 'new(...)'

using SocketMeister.Messages;
using System.Collections.Generic;
using System.Linq;

namespace SocketMeister
{
    /// <summary>
    /// Threadsafe list of open messages, which we are waiting for a response.
    /// </summary>
    internal class UnrespondedMessageCollection
    {
        private readonly List<MessageV1> _list = new List<MessageV1>();
        private readonly object _lock = new object();

        public MessageV1 this[long RequestID]
        {
            get
            {
                lock (_lock)
                {
                    foreach (MessageV1 message in _list)
                    {
                        if (message.MessageId == RequestID) return message;
                    }
                }
                return null;
            }
        }


        internal void Add(MessageV1 AddItem)
        {
            lock (_lock) { _list.Add(AddItem); }
        }


        /// <summary>
        /// Number of open items.
        /// </summary>
        public int Count
        {
            get { lock (_lock) { return _list.Count; } }
        }


        internal void Remove(MessageV1 RemoveItem)
        {
            lock (_lock) { _list.Remove(RemoveItem); }
        }

        /// <summary>
        /// Resets all messages which do not have a response, to 'Unsent'. The locks on all items are released so unsent items can be resent when a connection is reestablished.
        /// </summary>
        internal void ResetToUnsent()
        {
            List<MessageV1> messages;
            lock (_lock)
            {
                messages = _list.ToList();
            }
            foreach (MessageV1 message in messages)
            {
                if (message.WaitForResponse) message.Status = MessageEngineDeliveryStatus.Unsent;
            }
        }
    }
}

#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression


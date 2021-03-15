#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0090 // Use 'new(...)'

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocketMeister.Messages;


namespace SocketMeister
{
    /// <summary>
    /// Threadsafe list of open request messages, which we are waiting for a response.
    /// </summary>
    internal class OpenRequestMessageCollection
    {
        private readonly List<Message> _list = new List<Message>();
        private readonly object _lock = new object();

        public Message this[long RequestID]
        {
            get
            {
                lock (_lock)
                {
                    foreach (Message message in _list)
                    {
                        if (message.RequestId == RequestID) return message;
                    }
                }
                return null;
            }
        }


        internal void Add(Message AddItem)
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


        internal void Remove(Message RemoveItem)
        {
            lock (_lock) { _list.Remove(RemoveItem); }
        }

        /// <summary>
        /// Resets all messages which do not have a response, to 'Unsent'. The locks on all items are released so unsent items can be resent when a connection is reestablished.
        /// </summary>
        internal void ResetToUnsent()
        {
            List<Message> messages;
            lock (_lock)
            {
                messages = _list.ToList();
            }
            foreach (Message message in messages)
            {
                if (message.WaitForResponse) message.Status = MessageStatus.Unsent;
            }
        }
    }
}

#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression


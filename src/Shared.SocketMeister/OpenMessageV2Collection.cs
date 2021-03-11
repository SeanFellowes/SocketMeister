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
    /// Threadsafe list of open  messages, which we are waiting for a response.
    /// </summary>
    internal class OpenMessageV2Collection
    {
        private readonly List<MessageV2> _list = new List<MessageV2>();
        private readonly object _lock = new object();

        public MessageV2 this[long MessageId]
        {
            get
            {
                lock (_lock)
                {
                    foreach (MessageV2 message in _list)
                    {
                        if (message.MessageId == MessageId) return message;
                    }
                }
                return null;
            }
        }


        internal void Add(MessageV2 AddItem)
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


        internal void Remove(MessageV2 RemoveItem)
        {
            lock (_lock) { _list.Remove(RemoveItem); }
        }

        /// <summary>
        /// Resets all messages which do not have a response, to 'Unsent'. The locks on all items are released so unsent items can be resent when a connection is reestablished.
        /// </summary>
        internal void ResetToUnsent()
        {
            List<MessageV2> messages;
            lock (_lock)
            {
                messages = _list.ToList();
            }
            foreach (MessageV2 message in messages)
            {
                if (message.WaitForResponse) message.Status = MessageStatus.Unsent;
            }
        }
    }
}

#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression


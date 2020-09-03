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
        private readonly List<RequestMessage> _list = new List<RequestMessage>();
        private readonly object _lock = new object();

        public RequestMessage this[long RequestID]
        {
            get
            {
                lock (_lock)
                {
                    foreach (RequestMessage message in _list)
                    {
                        if (message.RequestId == RequestID) return message;
                    }
                }
                return null;
            }
        }


        internal void Add(RequestMessage AddItem)
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


        internal void Remove(RequestMessage RemoveItem)
        {
            lock (_lock) { _list.Remove(RemoveItem); }
        }

        /// <summary>
        /// Resets all messages which do not have a response, to 'Unsent'. The locks on all items are released so unsent items can be resent when a connection is reestablished.
        /// </summary>
        internal void ResetToUnsent()
        {
            List<RequestMessage> messages;
            lock (_lock)
            {
                messages = _list.ToList();
            }
            foreach (RequestMessage message in messages)
            {
                if (message.WaitForResponse) message.Status = MessageStatus.Unsent;
            }
        }
    }
}

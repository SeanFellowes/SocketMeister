using SocketMeister.Messages;
using System.Collections.Generic;

namespace SocketMeister
{
    internal class UnrespondedMessageCollection
    {
        private readonly Dictionary<long, MessageV1> _messages = new Dictionary<long, MessageV1>();
        private readonly object _lock = new object();

        public MessageV1 this[long RequestID]
        {
            get
            {
                lock (_lock)
                {
                    _messages.TryGetValue(RequestID, out MessageV1 message);
                    return message;
                }
            }
        }

        internal void Add(MessageV1 AddItem)
        {
            lock (_lock) { _messages.Add(AddItem.MessageId, AddItem); }
        }

        public int Count
        {
            get { lock (_lock) { return _messages.Count; } }
        }

        internal void Remove(MessageV1 RemoveItem)
        {
            lock (_lock) { _messages.Remove(RemoveItem.MessageId); }
        }

        internal void ResetToUnsent()
        {
            lock (_lock)
            {
                foreach (var message in _messages.Values)
                {
                    if (message.WaitForResponse) message.Status = MessageEngineDeliveryStatus.Unsent;
                }
            }
        }
    }
}

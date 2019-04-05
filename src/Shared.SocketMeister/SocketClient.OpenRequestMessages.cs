using System;
using System.Collections.Generic;
using SocketMeister.Messages;


namespace SocketMeister
{
    /// <summary>
    /// Asynchronous, persistent TCP/IP socket client supporting multiple destinations
    /// </summary>
#if SMISPUBLIC
    public partial class SocketClient : IDisposable
#else
    internal partial class SocketClient : IDisposable
#endif
    {
        /// <summary>
        /// Threadsafe list of open request messages, which we are waiting for a response from the socket server.
        /// </summary>
        internal class OpenRequestMessages
        {
            private readonly List<RequestMessage> _list = new List<RequestMessage>();
            private readonly object _lock = new object();

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


            internal RequestMessage Find(long RequestID)
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

            internal void Remove(RequestMessage RemoveItem)
            {
                lock (_lock) { _list.Remove(RemoveItem); }
            }

            /// <summary>
            /// Resets all messages which do not have a response, to 'Unsent'. The locks on all items are released so unsent items can be resent when a connection is reestablished.
            /// </summary>
            internal void ResetToUnsent()
            {
                lock (_lock)
                {
                    foreach (RequestMessage message in _list)
                    {
                        if (message.SendReceiveStatus != SendReceiveStatus.ResponseReceived) message.SendReceiveStatus = SendReceiveStatus.Unsent;
                    }
                }
            }
        }
    }
}

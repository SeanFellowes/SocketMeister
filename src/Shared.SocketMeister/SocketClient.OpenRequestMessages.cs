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
            private readonly object classLock = new object();
            private readonly List<RequestMessage> list = new List<RequestMessage>();

            internal void Add(RequestMessage addItem)
            {
                lock (classLock) { list.Add(addItem); }
            }

            /// <summary>
            /// Number of open items.
            /// </summary>
            public int Count
            {
                get { lock (classLock) { return list.Count; } }
            }


            internal RequestMessage Find(long requestID)
            {
                lock (classLock)
                {
                    foreach (RequestMessage message in list)
                    {
                        if (message.RequestId == requestID) return message;
                    }
                }
                return null;
            }

            internal void Remove(RequestMessage removeItem)
            {
                lock (classLock) { list.Remove(removeItem); }
            }

            /// <summary>
            /// Resets all messages which do not have a response, to 'Unsent'. The locks on all items are released so unsent items can be resent when a connection is reestablished.
            /// </summary>
            internal void ResetToUnsent()
            {
                lock (classLock)
                {
                    foreach (RequestMessage message in list)
                    {
                        if (message.SendReceiveStatus != SendReceiveStatus.ResponseReceived) message.SendReceiveStatus = SendReceiveStatus.Unsent;
                    }
                }
            }
        }
    }
}

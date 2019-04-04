using System;
using System.Collections.Generic;
using System.Net.Sockets;

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
        /// Based on example from http://msdn2.microsoft.com/en-us/library/system.net.sockets.socketasynceventargs.socketasynceventargs.aspx
        /// Represents a collection of reusable SocketAsyncEventArgs objects.  
        /// </summary>
        internal sealed class SocketAsyncEventArgsPool
        {
            /// <summary>
            /// Pool of SocketAsyncEventArgs.
            /// </summary>
            private readonly Stack<SocketAsyncEventArgs> pool;

            /// <summary>
            /// Initializes the object pool to the specified size.
            /// </summary>
            /// <param name="Capacity">Maximum number of SocketAsyncEventArgs objects the pool can hold.</param>
            internal SocketAsyncEventArgsPool(int Capacity)
            {
                this.pool = new Stack<SocketAsyncEventArgs>(Capacity);
            }

            /// <summary>
            /// Removes a SocketAsyncEventArgs instance from the pool.
            /// </summary>
            /// <returns>SocketAsyncEventArgs removed from the pool.</returns>
            internal SocketAsyncEventArgs Pop()
            {
                lock (this.pool)
                {
                    //  ENSURE THERE IS ALWAYS AT LEAST ONE ITEM
                    if (this.pool.Count > 0) return this.pool.Pop();
                    return null;
                }
            }

            /// <summary>
            /// Add a SocketAsyncEventArg instance to the pool. 
            /// </summary>
            /// <param name="item">SocketAsyncEventArgs instance to add to the pool.</param>
            internal void Push(SocketAsyncEventArgs item)
            {
                if (item == null) return;
                lock (this.pool)
                {
                    if (pool.Contains(item) == true) return;
                    this.pool.Push(item);
                }
            }

        }
    }
}

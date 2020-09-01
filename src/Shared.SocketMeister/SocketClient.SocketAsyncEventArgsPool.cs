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
            private readonly Stack<SocketAsyncEventArgs> _pool;

            /// <summary>
            /// The event used to complete an asynchronous operation.
            /// </summary>
            public event EventHandler<SocketAsyncEventArgs> Completed;

            /// <summary>
            /// Initializes the object pool to the specified size.
            /// </summary>
            /// <param name="Capacity">Maximum number of SocketAsyncEventArgs objects the pool can hold.</param>
            internal SocketAsyncEventArgsPool()
            {
                this._pool = new Stack<SocketAsyncEventArgs>(Constants.SocketAsyncEventArgsPoolSize);
                for (int i = 0; i < Constants.SocketAsyncEventArgsPoolSize; i++)
                {
#pragma warning disable CA2000 // Dispose objects before losing scope
                    SocketAsyncEventArgs EventArgs = new SocketAsyncEventArgs();
#pragma warning restore CA2000 // Dispose objects before losing scope
                    EventArgs.SetBuffer(new byte[SEND_RECEIVE_BUFFER_SIZE], 0, SEND_RECEIVE_BUFFER_SIZE);
                    EventArgs.Completed += EventArgs_Completed; ;
                    this._pool.Push(EventArgs);
                }
            }

            private void EventArgs_Completed(object sender, SocketAsyncEventArgs e)
            {
                Completed?.Invoke(sender, e);
            }



            /// <summary>
            /// Removes a SocketAsyncEventArgs instance from the pool.
            /// </summary>
            /// <returns>SocketAsyncEventArgs removed from the pool.</returns>
            internal SocketAsyncEventArgs Pop()
            {
                lock (this._pool)
                {
                    //  ENSURE THERE IS ALWAYS AT LEAST ONE ITEM
                    if (this._pool.Count > 0) return this._pool.Pop();
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
                lock (this._pool)
                {
                    if (_pool.Contains(item) == true) return;
                    this._pool.Push(item);
                }
            }

        }
    }
}

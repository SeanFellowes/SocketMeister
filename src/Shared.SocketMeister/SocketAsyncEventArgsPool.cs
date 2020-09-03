using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace SocketMeister
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
        internal SocketAsyncEventArgsPool(int Capacity)
        {
            _pool = new Stack<SocketAsyncEventArgs>(Capacity);
            for (int i = 0; i < Capacity; i++)
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                SocketAsyncEventArgs EventArgs = new SocketAsyncEventArgs();
#pragma warning restore CA2000 // Dispose objects before losing scope
                EventArgs.SetBuffer(new byte[Constants.SEND_RECEIVE_BUFFER_SIZE], 0, Constants.SEND_RECEIVE_BUFFER_SIZE);
                EventArgs.Completed += EventArgs_Completed; ;
                _pool.Push(EventArgs);
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
            lock (_pool)
            {
                //  ENSURE THERE IS ALWAYS AT LEAST ONE ITEM
                if (_pool.Count > 0) return _pool.Pop();
                return null;
            }
        }


        /// <summary>
        /// Removes a SocketAsyncEventArgs instance from the pool.
        /// </summary>
        /// <returns>SocketAsyncEventArgs removed from the pool.</returns>
        internal SocketAsyncEventArgs Pop(int MaxTryMilliseconds)
        {
            while (true == true)
            lock (_pool)
            {
                //  ENSURE THERE IS ALWAYS AT LEAST ONE ITEM
                if (_pool.Count > 0) return _pool.Pop();
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
            lock (_pool)
            {
                if (_pool.Contains(item) == false) _pool.Push(item);
            }
        }

    }
}

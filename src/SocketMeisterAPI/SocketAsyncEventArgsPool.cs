using SocketMeister;
using System.Collections.Generic;
using System.Net.Sockets;
using System;

/// <summary>
/// A reusable pool of <see cref="SocketAsyncEventArgs"/> objects to optimize socket operations by reducing allocations.
/// </summary>
internal sealed class SocketAsyncEventArgsPool : IDisposable
{
    private readonly Stack<SocketAsyncEventArgs> _pool; // Stack to hold reusable SocketAsyncEventArgs objects
    private readonly byte[] _sharedBuffer; // Shared buffer used by all SocketAsyncEventArgs instances
    private bool _disposed = false; // Tracks whether the object has been disposed

    /// <summary>
    /// Event triggered when a <see cref="SocketAsyncEventArgs"/> operation is completed.
    /// </summary>
    public event EventHandler<SocketAsyncEventArgs> Completed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SocketAsyncEventArgsPool"/> class.
    /// Pre-allocates a pool of <see cref="SocketAsyncEventArgs"/> objects and a shared buffer.
    /// </summary>
    internal SocketAsyncEventArgsPool()
    {
        // Initialize the pool with a predefined size
        _pool = new Stack<SocketAsyncEventArgs>(Constants.SOCKET_ASYNC_EVENT_ARGS_POOL_SIZE);

        // Allocate a shared buffer for all SocketAsyncEventArgs instances
        _sharedBuffer = new byte[Constants.SOCKET_ASYNC_EVENT_ARGS_POOL_SIZE * Constants.SEND_RECEIVE_BUFFER_SIZE];

        // Pre-allocate and configure SocketAsyncEventArgs objects
        for (int i = 0; i < Constants.SOCKET_ASYNC_EVENT_ARGS_POOL_SIZE; i++)
        {
            var eventArgs = new SocketAsyncEventArgs();

            // Assign a segment of the shared buffer to each SocketAsyncEventArgs
            eventArgs.SetBuffer(_sharedBuffer, i * Constants.SEND_RECEIVE_BUFFER_SIZE, Constants.SEND_RECEIVE_BUFFER_SIZE);

            // Attach the Completed event handler
            eventArgs.Completed += EventArgs_Completed;

            // Add the configured SocketAsyncEventArgs to the pool
            _pool.Push(eventArgs);
        }
    }

    /// <summary>
    /// Handles the <see cref="SocketAsyncEventArgs.Completed"/> event and forwards it to subscribers.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SocketAsyncEventArgs"/> instance associated with the event.</param>
    private void EventArgs_Completed(object sender, SocketAsyncEventArgs e)
    {
        Completed?.Invoke(sender, e);
    }

    /// <summary>
    /// Retrieves a <see cref="SocketAsyncEventArgs"/> object from the pool.
    /// </summary>
    /// <returns>A <see cref="SocketAsyncEventArgs"/> object, or <c>null</c> if the pool is empty.</returns>
    internal SocketAsyncEventArgs Pop()
    {
        lock (_pool)
        {
            return _pool.Count > 0 ? _pool.Pop() : null;
        }
    }

    /// <summary>
    /// Returns a <see cref="SocketAsyncEventArgs"/> object to the pool after use.
    /// </summary>
    /// <param name="item">The <see cref="SocketAsyncEventArgs"/> object to return to the pool.</param>
    internal void Push(SocketAsyncEventArgs item)
    {
        if (item == null) return; // Ignore null items

        lock (_pool)
        {
            _pool.Push(item);
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="SocketAsyncEventArgsPool"/> instance.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return; // Prevent multiple disposals

        lock (_pool)
        {
            // Dispose of all SocketAsyncEventArgs objects in the pool
            while (_pool.Count > 0)
            {
                var eventArgs = _pool.Pop();

                // Detach the Completed event handler to avoid memory leaks
                eventArgs.Completed -= EventArgs_Completed;

                // Dispose of the SocketAsyncEventArgs object
                eventArgs.Dispose();
            }
        }

        _disposed = true; // Mark the object as disposed
    }
}

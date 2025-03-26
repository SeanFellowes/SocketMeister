using SocketMeister;
using System.Collections.Generic;
using System.Net.Sockets;
using System;

internal sealed class SocketAsyncEventArgsPool : IDisposable
{
    private readonly Stack<SocketAsyncEventArgs> _pool;
    private readonly byte[] _sharedBuffer; // Reference to the shared buffer
    private bool _disposed = false;

    public event EventHandler<SocketAsyncEventArgs> Completed;

    internal SocketAsyncEventArgsPool()
    {
        _pool = new Stack<SocketAsyncEventArgs>(Constants.SOCKET_ASYNC_EVENT_ARGS_POOL_SIZE);
        _sharedBuffer = new byte[Constants.SOCKET_ASYNC_EVENT_ARGS_POOL_SIZE * Constants.SEND_RECEIVE_BUFFER_SIZE];

        for (int i = 0; i < Constants.SOCKET_ASYNC_EVENT_ARGS_POOL_SIZE; i++)
        {
            var eventArgs = new SocketAsyncEventArgs();
            eventArgs.SetBuffer(_sharedBuffer, i * Constants.SEND_RECEIVE_BUFFER_SIZE, Constants.SEND_RECEIVE_BUFFER_SIZE);
            eventArgs.Completed += EventArgs_Completed;
            _pool.Push(eventArgs);
        }
    }

    private void EventArgs_Completed(object sender, SocketAsyncEventArgs e)
    {
        Completed?.Invoke(sender, e);
    }

    internal SocketAsyncEventArgs Pop()
    {
        lock (_pool)
        {
            return _pool.Count > 0 ? _pool.Pop() : null;
        }
    }

    internal void Push(SocketAsyncEventArgs item)
    {
        if (item == null) return;

        lock (_pool)
        {
            _pool.Push(item);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        lock (_pool)
        {
            while (_pool.Count > 0)
            {
                var eventArgs = _pool.Pop();
                eventArgs.Completed -= EventArgs_Completed; // Remove handler
                eventArgs.Dispose(); // Dispose of unmanaged resources
            }
        }

        _disposed = true;
    }
}

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SocketMeister.Tests.Common;

public sealed class EventSink<T>
{
    private readonly ConcurrentQueue<T> _events = new();
    private readonly SemaphoreSlim _signal = new(0);

    public void Add(T evt)
    {
        _events.Enqueue(evt);
        _signal.Release();
    }

    public bool TryDequeue(out T evt) => _events.TryDequeue(out evt);

    public async Task<T> WaitAsync(TimeSpan timeout)
    {
        if (!await _signal.WaitAsync(timeout).ConfigureAwait(false))
            throw new TimeoutException("Timed out waiting for event.");
        if (!_events.TryDequeue(out var evt))
            throw new InvalidOperationException("Signal received but queue was empty.");
        return evt;
    }
}


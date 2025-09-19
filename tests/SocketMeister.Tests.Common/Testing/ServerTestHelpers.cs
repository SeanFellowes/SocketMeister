using System;
using System.Threading.Tasks;
using SocketMeister;

namespace SocketMeister.Tests.Common;

public static class ServerTestHelpers
{
    public static async Task WaitForServerStartedAsync(SocketServer server, int timeoutMs = 5000)
    {
        if (server == null) throw new ArgumentNullException(nameof(server));
        if (server.IsRunning) return;

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        EventHandler<SocketServer.ServerStatusChangedEventArgs> handler = null!;
        handler = (s, e) =>
        {
            if (e.NewStatus == SocketServerStatus.Started)
                tcs.TrySetResult(true);
        };
        server.StatusChanged += handler;
        try
        {
            await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs)).ConfigureAwait(false);
        }
        finally
        {
            server.StatusChanged -= handler;
        }
    }
}


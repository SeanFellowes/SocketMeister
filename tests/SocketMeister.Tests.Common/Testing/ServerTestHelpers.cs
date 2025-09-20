using System;
using System.Threading.Tasks;
using SocketMeister;

namespace SocketMeister.Tests.Common;

public static class ServerTestHelpers
{
    public static int GetBoundPort(SocketServer server)
    {
        if (server == null) throw new ArgumentNullException(nameof(server));
        var ep = server.EndPoint;
        if (string.IsNullOrEmpty(ep)) return 0;
        int idx = ep.LastIndexOf(':');
        if (idx < 0 || idx == ep.Length - 1) return 0;
        if (int.TryParse(ep.Substring(idx + 1), out var port)) return port;
        return 0;
    }
    public static async Task WaitForServerStartedAsync(SocketServer server, int timeoutMs = 10000)
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
            // Re-check after subscribing to avoid race if server already started
            if (server.IsRunning) tcs.TrySetResult(true);
            await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs)).ConfigureAwait(false);
        }
        finally
        {
            server.StatusChanged -= handler;
        }
    }
}

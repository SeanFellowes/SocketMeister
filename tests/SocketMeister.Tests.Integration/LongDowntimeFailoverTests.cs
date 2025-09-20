using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class LongDowntimeFailoverTests
{
    private static async Task WaitConnectedAsync(SocketClient client, int port, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        while (!cts.IsCancellationRequested)
        {
            if (client.ConnectionStatus == SocketClient.ConnectionStatuses.Connected && client.CurrentEndPoint?.Port == port) return;
            await Task.Delay(200);
        }
        throw new TimeoutException("Connect timeout");
    }

    [Trait("Category","Failover")]
    [Fact]
    public async Task Long_Downtime_Then_Reconnect()
    {
        using var r1 = PortReservation.ReserveLoopbackPort();
        using var r2 = PortReservation.ReserveLoopbackPort();
        int port1 = r1.Port;
        int port2 = r2.Port;

        var server1 = new SocketServer(port1, false);
        r1.Dispose();
        server1.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server1);
            await Task.Delay(200);
        var server2 = new SocketServer(port2, false);
        try
        {
            var client = new SocketClient(new List<SocketEndPoint>
            {
                new SocketEndPoint("127.0.0.1", port1),
                new SocketEndPoint("127.0.0.1", port2)
            }, false, "LongDowntime");

            client.Start();
            await WaitConnectedAsync(client, port1, TimeSpan.FromSeconds(60));

            // Bring everything down for a while
            server1.Stop();
            await Task.Delay(10000); // simulate downtime

            // Bring up server2 and expect reconnect
            r2.Dispose();
            server2.Start();
            await ServerTestHelpers.WaitForServerStartedAsync(server2);
            await Task.Delay(200);

            // Event-driven reconnect wait targeting port2
            var reconnected = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            EventHandler<SocketClient.ConnectionStatusChangedEventArgs> handler = null!;
            handler = (s, e) =>
            {
                if (e.NewStatus == SocketClient.ConnectionStatuses.Connected && client.CurrentEndPoint?.Port == port2)
                    reconnected.TrySetResult(true);
            };
            client.ConnectionStatusChanged += handler;
            if (client.ConnectionStatus == SocketClient.ConnectionStatuses.Connected && client.CurrentEndPoint?.Port == port2)
                reconnected.TrySetResult(true);
            await Task.WhenAny(reconnected.Task, Task.Delay(TimeSpan.FromSeconds(90)));
            client.ConnectionStatusChanged -= handler;
            Assert.True(reconnected.Task.IsCompleted, $"Client did not reconnect to port {port2}");

            client.Stop();
        }
        finally
        {
            try { server1.Stop(); } catch { }
            try { server2.Stop(); } catch { }
            (server1 as IDisposable)?.Dispose();
            (server2 as IDisposable)?.Dispose();
        }
    }
}



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
        int port1 = PortAllocator.GetFreeTcpPort();
        int port2 = PortAllocator.GetFreeTcpPort();

        var server1 = new SocketServer(port1, false);
        server1.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server1);
        var server2 = new SocketServer(port2, false);
        try
        {
            var client = new SocketClient(new List<SocketEndPoint>
            {
                new SocketEndPoint("127.0.0.1", port1),
                new SocketEndPoint("127.0.0.1", port2)
            }, false, "LongDowntime");

            client.Start();
            await WaitConnectedAsync(client, port1, TimeSpan.FromSeconds(30));

            // Bring everything down for a while
            server1.Stop();
            await Task.Delay(10000); // simulate downtime

            // Bring up server2 and expect reconnect
            server2.Start();
            await ServerTestHelpers.WaitForServerStartedAsync(server2);
            await WaitConnectedAsync(client, port2, TimeSpan.FromSeconds(45));

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

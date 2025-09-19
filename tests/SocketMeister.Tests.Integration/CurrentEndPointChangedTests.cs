using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class CurrentEndPointChangedTests
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

    [Trait("Category","EndpointChanged")]
    [Fact]
    public async Task Sequence_On_Failover_And_Failback()
    {
        int port1 = PortAllocator.GetFreeTcpPort();
        int port2 = PortAllocator.GetFreeTcpPort();
        var s1 = new SocketServer(port1, false);
        var s2 = new SocketServer(port2, false);

        var sequence = new List<ushort>();

        // Start s1
        s1.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(s1);
        try
        {
            var client = new SocketClient(new List<SocketEndPoint>
            {
                new SocketEndPoint("127.0.0.1", port1),
                new SocketEndPoint("127.0.0.1", port2)
            }, false, "EPChange");

            client.CurrentEndPointChanged += (s, e) =>
            {
                if (e.NewEndPoint != null) sequence.Add(e.NewEndPoint.Port);
            };

            client.Start();
            await WaitConnectedAsync(client, port1, TimeSpan.FromSeconds(30));

            // Switch to s2
            s1.Stop();
            await Task.Delay(500);
            s2.Start();
            await ServerTestHelpers.WaitForServerStartedAsync(s2);
            await WaitConnectedAsync(client, port2, TimeSpan.FromSeconds(45));

            // Switch back to s1
            s2.Stop();
            await Task.Delay(500);
            s1.Start();
            await ServerTestHelpers.WaitForServerStartedAsync(s1);
            await WaitConnectedAsync(client, port1, TimeSpan.FromSeconds(45));

            Assert.Contains((ushort)port2, sequence);
            Assert.Contains((ushort)port1, sequence);
            // Ensure the last event corresponds to current port1
            Assert.Equal(port1, sequence.Last());

            client.Stop();
        }
        finally
        {
            try { s1.Stop(); } catch { }
            try { s2.Stop(); } catch { }
            (s1 as IDisposable)?.Dispose();
            (s2 as IDisposable)?.Dispose();
        }
    }
}

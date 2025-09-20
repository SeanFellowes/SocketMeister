using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class AlternatingFailoverTests
{
    private static async Task WaitConnectedAsync(SocketClient client, int port, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        while (!cts.IsCancellationRequested)
        {
            if (client.ConnectionStatus == SocketClient.ConnectionStatuses.Connected && client.CurrentEndPoint?.Port == port) return;
            await Task.Delay(200);
        }
        throw new TimeoutException($"Connect to {port} timeout");
    }

    [Trait("Category","Failover")]
    [Fact]
    public async Task Alternating_Server_Failover_Twice()
    {
        using var r1 = PortReservation.ReserveLoopbackPort();
        using var r2 = PortReservation.ReserveLoopbackPort();
        int port1 = r1.Port;
        int port2 = r2.Port;

        var s1 = new SocketServer(port1, false);
        var s2 = new SocketServer(port2, false);

        // Start s1 only
        r1.Dispose();
        s1.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(s1);
            await Task.Delay(200);
        try
        {
            var client = new SocketClient(new List<SocketEndPoint>
            {
                new SocketEndPoint("127.0.0.1", port1),
                new SocketEndPoint("127.0.0.1", port2)
            }, false, "Alternate");
            client.Start();

            await WaitConnectedAsync(client, port1, TimeSpan.FromSeconds(60));

            // Cycle 1: switch to s2
            s1.Stop();
            await Task.Delay(500);
            r2.Dispose();
            s2.Start();
            await ServerTestHelpers.WaitForServerStartedAsync(s1);
            await Task.Delay(200);
            await WaitConnectedAsync(client, port2, TimeSpan.FromSeconds(60));

            // Cycle 2: switch back to s1
            s2.Stop();
            await Task.Delay(500);
            s1.Start();
            await ServerTestHelpers.WaitForServerStartedAsync(s1);
            await Task.Delay(200);
            await WaitConnectedAsync(client, port1, TimeSpan.FromSeconds(60));

            // One more flip to s2 for robustness
            s1.Stop();
            await Task.Delay(500);
            s2.Start();
            await ServerTestHelpers.WaitForServerStartedAsync(s1);
            await Task.Delay(200);
            await WaitConnectedAsync(client, port2, TimeSpan.FromSeconds(60));

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




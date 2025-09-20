using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class ExtendedFailoverTests
{
    private static async Task WaitConnectedToPortAsync(SocketClient client, int port, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        while (!cts.IsCancellationRequested)
        {
            if (client.ConnectionStatus == SocketClient.ConnectionStatuses.Connected && client.CurrentEndPoint?.Port == port)
                return;
            await Task.Delay(200);
        }
        throw new TimeoutException($"Client did not connect to port {port} within {timeout}.");
    }

    [Trait("Category","Failover")]
    [Fact]
    public async Task Delayed_Server_Start_And_Failback()
    {
        using var r1 = PortReservation.ReserveLoopbackPort();
        using var r2 = PortReservation.ReserveLoopbackPort();
        int port1 = r1.Port;
        int port2 = r2.Port;

        // Client has both endpoints; no servers up yet
        var client = new SocketClient(new List<SocketEndPoint>
        {
            new SocketEndPoint("127.0.0.1", port1),
            new SocketEndPoint("127.0.0.1", port2)
        }, false, "ExtendedFailover");

        client.Start();

        // After some seconds, start server2 only
        await Task.Delay(3000);
        r2.Dispose();
        var server2 = new SocketServer(port2, false);
        server2.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server2);
            await Task.Delay(200);
        try
        {
            // Allow up to 45s for backoff/eligibility windows to align
            await WaitConnectedToPortAsync(client, port2, TimeSpan.FromSeconds(90));

            // Now stop server2 and start server1; expect client to reconnect to port1 eventually
            server2.Stop();
            await Task.Delay(1000);
            r1.Dispose();
            var server1 = new SocketServer(port1, false);
            server1.Start();
            await ServerTestHelpers.WaitForServerStartedAsync(server1);
            await Task.Delay(200);
            try
            {
                await WaitConnectedToPortAsync(client, port1, TimeSpan.FromSeconds(90));
            }
            finally
            {
                try { server1.Stop(); } catch { }
                (server1 as IDisposable)?.Dispose();
            }
        }
        finally
        {
            try { server2.Stop(); } catch { }
            (server2 as IDisposable)?.Dispose();
            try { client.Stop(); } catch { }
        }
    }
}



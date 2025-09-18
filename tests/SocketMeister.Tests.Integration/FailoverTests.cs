using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class FailoverTests
{
    private static Task WaitForConnectedAsync(SocketClient client, TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        EventHandler<SocketClient.ConnectionStatusChangedEventArgs> handler = null!;
        handler = (s, e) =>
        {
            if (e.NewStatus == SocketClient.ConnectionStatuses.Connected)
            {
                client.ConnectionStatusChanged -= handler;
                tcs.TrySetResult(true);
            }
        };
        client.ConnectionStatusChanged += handler;
        client.Start();
        return Task.WhenAny(tcs.Task, Task.Delay(timeout)).ContinueWith(t =>
        {
            if (tcs.Task.IsCompleted) return;
            client.ConnectionStatusChanged -= handler;
            throw new TimeoutException("Client did not reach Connected state in time.");
        });
    }

    [Trait("Category","Failover")]
    [Fact]
    public async Task Client_FailsOver_Between_Two_Endpoints()
    {
        int port1 = PortAllocator.GetFreeTcpPort();
        int port2 = PortAllocator.GetFreeTcpPort();

        var server1 = new SocketServer(port1, CompressSentData: false);
        var server2 = new SocketServer(port2, CompressSentData: false);

        try
        {
            // Start only server1
            server1.Start();

            var endpoints = new List<SocketEndPoint>
            {
                new SocketEndPoint("127.0.0.1", port1),
                new SocketEndPoint("127.0.0.1", port2)
            };
            var client = new SocketClient(endpoints, EnableCompression: false, FriendlyName: "FailoverClient");

            var epChanges = new List<int>();
            client.CurrentEndPointChanged += (s, e) =>
            {
                if (e.NewEndPoint != null)
                    epChanges.Add(e.NewEndPoint.Port);
            };

            await WaitForConnectedAsync(client, TimeSpan.FromSeconds(10));
            Assert.Equal(port1, client.CurrentEndPoint.Port);

            // Stop server1, wait a moment, then start server2
            server1.Stop();
            await Task.Delay(1000);
            server2.Start();

            // The client should reconnect to server2
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            while (!cts.IsCancellationRequested)
            {
                if (client.ConnectionStatus == SocketClient.ConnectionStatuses.Connected && client.CurrentEndPoint?.Port == port2)
                    break;
                await Task.Delay(200);
            }

            Assert.Equal(SocketClient.ConnectionStatuses.Connected, client.ConnectionStatus);
            Assert.Equal(port2, client.CurrentEndPoint.Port);
            Assert.Contains(port2, epChanges);

            // Now stop server2 and start server1 again; expect reconnection back to port1
            server2.Stop();
            await Task.Delay(1000);
            server1.Start();

            cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            while (!cts.IsCancellationRequested)
            {
                if (client.ConnectionStatus == SocketClient.ConnectionStatuses.Connected && client.CurrentEndPoint?.Port == port1)
                    break;
                await Task.Delay(200);
            }

            Assert.Equal(SocketClient.ConnectionStatuses.Connected, client.ConnectionStatus);
            Assert.Equal(port1, client.CurrentEndPoint.Port);
            Assert.Contains(port1, epChanges);

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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class PubSubTests
{
    private static SocketClient NewClient(string host, int port, string name)
        => new SocketClient(new List<SocketEndPoint> { new SocketEndPoint(host, port) }, false, name);

    private static Task WaitConnectedAsync(SocketClient client, TimeSpan timeout)
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
            if (!tcs.Task.IsCompleted)
            {
                client.ConnectionStatusChanged -= handler;
                throw new TimeoutException("Connect timeout");
            }
        });
    }

    [Trait("Category","PubSub")]
    [Fact]
    public async Task Broadcast_Received_By_All_Subscribers()
    {
        int port = PortAllocator.GetFreeTcpPort();
        var server = new SocketServer(port, false);
        server.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server);
        try
        {
            var c1 = NewClient("127.0.0.1", port, "c1");
            var c2 = NewClient("127.0.0.1", port, "c2");
            var c3 = NewClient("127.0.0.1", port, "c3");

            var sink1 = new List<(string name, object[] parameters)>();
            var sink2 = new List<(string name, object[] parameters)>();
            var sink3 = new List<(string name, object[] parameters)>();
            var allReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            int received = 0;
            EventHandler<SocketClient.BroadcastReceivedEventArgs> handler = (s, e) =>
            {
                if (Interlocked.Increment(ref received) >= 3) allReceived.TrySetResult(true);
            };
            c1.BroadcastReceived += (s, e) => { sink1.Add((e.Name, e.Parameters)); handler(s, e); };
            c2.BroadcastReceived += (s, e) => { sink2.Add((e.Name, e.Parameters)); handler(s, e); };
            c3.BroadcastReceived += (s, e) => { sink3.Add((e.Name, e.Parameters)); handler(s, e); };

            // Small settling delay after server starts on CI runners
            await Task.Delay(200);
            await Task.WhenAll(
                WaitConnectedAsync(c1, TimeSpan.FromSeconds(60)),
                WaitConnectedAsync(c2, TimeSpan.FromSeconds(60)),
                WaitConnectedAsync(c3, TimeSpan.FromSeconds(60))
            );

            // Subscribe all 3 to same topic
            var topic = "news";
            c1.AddSubscription(topic);
            c2.AddSubscription(topic);
            c3.AddSubscription(topic);

            // Allow token change propagation
            await Task.Delay(1000);

            // Send broadcast
            var msg = new object[] { "hello" };
            server.BroadcastToSubscribers(topic, msg);

            await Task.WhenAny(allReceived.Task, Task.Delay(TimeSpan.FromSeconds(10)));
            Assert.True(allReceived.Task.IsCompleted, "Not all clients received broadcast");
            Assert.Single(sink1);
            Assert.Single(sink2);
            Assert.Single(sink3);
            Assert.Equal("news", sink1[0].name);
            Assert.Equal("hello", sink1[0].parameters[0] as string);

            c1.Stop(); c2.Stop(); c3.Stop();
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }

    [Trait("Category","PubSub")]
    [Fact]
    public async Task Broadcast_Routed_To_Correct_Subscribers()
    {
        int port = PortAllocator.GetFreeTcpPort();
        var server = new SocketServer(port, false);
        server.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server);
        try
        {
            var c1 = NewClient("127.0.0.1", port, "c1");
            var c2 = NewClient("127.0.0.1", port, "c2");
            var c3 = NewClient("127.0.0.1", port, "c3");

            int r1=0, r2=0, r3=0;
            c1.BroadcastReceived += (s, e) => Interlocked.Increment(ref r1);
            c2.BroadcastReceived += (s, e) => Interlocked.Increment(ref r2);
            c3.BroadcastReceived += (s, e) => Interlocked.Increment(ref r3);

            await Task.WhenAll(
                WaitConnectedAsync(c1, TimeSpan.FromSeconds(60)),
                WaitConnectedAsync(c2, TimeSpan.FromSeconds(60)),
                WaitConnectedAsync(c3, TimeSpan.FromSeconds(60))
            );

            c1.AddSubscription("A");
            c2.AddSubscription("B");
            c3.AddSubscription("C");

            await Task.Delay(1000);

            server.BroadcastToSubscribers("A", new object[] { 1 });
            server.BroadcastToSubscribers("B", new object[] { 2 });
            server.BroadcastToSubscribers("C", new object[] { 3 });
            server.BroadcastToSubscribers("Z", new object[] { 9 }); // no one

            await Task.Delay(500);

            Assert.Equal(1, r1);
            Assert.Equal(1, r2);
            Assert.Equal(1, r3);

            c1.Stop(); c2.Stop(); c3.Stop();
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }

    [Trait("Category","PubSub")]
    [Fact]
    public async Task RemoveSubscription_Stops_Routing_And_Survives_Restart()
    {
        int port = PortAllocator.GetFreeTcpPort();
        var server = new SocketServer(port, false);
        server.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server);
        try
        {
            var client = NewClient("127.0.0.1", port, "client");
            int received = 0;
            client.BroadcastReceived += (s, e) => Interlocked.Increment(ref received);
            await WaitConnectedAsync(client, TimeSpan.FromSeconds(30));

            client.AddSubscription("T1");
            client.AddSubscription("T2");
            await Task.Delay(600);
            Assert.True(server.DoSubscribersExist("T1"));
            Assert.True(server.DoSubscribersExist("T2"));

            server.BroadcastToSubscribers("T1", new object[] { "x" });
            await Task.Delay(600);
            Assert.Equal(1, received);

            // Remove T1
            Assert.True(client.RemoveSubscription("T1"));
            await Task.Delay(300);
            server.BroadcastToSubscribers("T1", new object[] { "y" });
            await Task.Delay(300);
            Assert.Equal(1, received); // unchanged

            // Restart server; T2 should be re-sent by handshake; T1 should remain removed
            server.Stop();
            await Task.Delay(1000);
            server.Start();
            await ServerTestHelpers.WaitForServerStartedAsync(server);

            // Wait for reconnect (allow ample time)
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            while (!cts.IsCancellationRequested && client.ConnectionStatus != SocketClient.ConnectionStatuses.Connected)
                await Task.Delay(200);
            Assert.Equal(SocketClient.ConnectionStatuses.Connected, client.ConnectionStatus);

            // Post-restart routing check
            Assert.False(server.DoSubscribersExist("T1"));
            Assert.True(server.DoSubscribersExist("T2"));
            server.BroadcastToSubscribers("T2", new object[] { "z" });
            await Task.Delay(500);
            Assert.True(received >= 2);

            client.Stop();
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }
}

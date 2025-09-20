using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class BroadcastAllTests
{
    [Trait("Category","Broadcast")]
    [Fact]
    public async Task Broadcast_To_All_Clients()
    {
        var server = new SocketServer(0, false);
        server.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server);
        int port = ServerTestHelpers.GetBoundPort(server);
        try
        {
            var c1 = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "BA1");
            var c2 = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "BA2");
            var c3 = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "BA3");

            int r1=0, r2=0, r3=0;
            c1.BroadcastReceived += (s, e) => Interlocked.Increment(ref r1);
            c2.BroadcastReceived += (s, e) => Interlocked.Increment(ref r2);
            c3.BroadcastReceived += (s, e) => Interlocked.Increment(ref r3);

            var t1 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var t2 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var t3 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            c1.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) t1.TrySetResult(true); };
            c2.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) t2.TrySetResult(true); };
            c3.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) t3.TrySetResult(true); };
            c1.Start(); c2.Start(); c3.Start();
            await Task.WhenAll(Task.WhenAny(t1.Task, Task.Delay(60000)), Task.WhenAny(t2.Task, Task.Delay(60000)), Task.WhenAny(t3.Task, Task.Delay(60000)));
            Assert.True(t1.Task.IsCompleted && t2.Task.IsCompleted && t3.Task.IsCompleted, "Clients did not connect");

            server.Broadcast("all", new object[] { 42 });
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

    [Trait("Category","Broadcast")]
    [Fact]
    public void BroadcastToSubscribers_EmptyName_Throws()
    {
        var server = new SocketServer(0, false);
        server.Start();
        try
        {
            Assert.Throws<ArgumentNullException>(() => server.BroadcastToSubscribers(null, new object[] { }));
            Assert.Throws<ArgumentNullException>(() => server.BroadcastToSubscribers("", new object[] { }));
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }
}

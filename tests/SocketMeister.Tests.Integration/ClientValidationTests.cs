using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class ClientValidationTests
{
    [Trait("Category","ClientValidation")]
    [Fact]
    public void SendMessage_NullParameters_Throws()
    {
        var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", 65000) }, false, "CV-Null");
        Assert.Throws<ArgumentException>(() => client.SendMessage(null));
    }

    [Trait("Category","ClientValidation")]
    [Fact]
    public void SendMessage_EmptyParameters_Throws()
    {
        var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", 65001) }, false, "CV-Empty");
        Assert.Throws<ArgumentException>(() => client.SendMessage(Array.Empty<object>()));
    }

    [Trait("Category","ClientValidation")]
    [Fact]
    public async Task AddSubscription_Duplicate_Throws()
    {
        int port = PortAllocator.GetFreeTcpPort();
        var server = new SocketServer(port, false);
        server.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server);
        try
        {
            var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "CV-SubDup");
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) tcs.TrySetResult(true); };
            client.Start();
            await Task.WhenAny(tcs.Task, Task.Delay(30000));
            Assert.True(tcs.Task.IsCompleted);

            client.AddSubscription("topic");
            Assert.Throws<ArgumentException>(() => client.AddSubscription("topic"));

            client.Stop();
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }

    [Trait("Category","ClientValidation")]
    [Fact]
    public async Task RemoveSubscription_Missing_ReturnsFalse()
    {
        int port = PortAllocator.GetFreeTcpPort();
        var server = new SocketServer(port, false);
        server.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server);
        try
        {
            var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "CV-Remove");
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) tcs.TrySetResult(true); };
            client.Start();
            await Task.WhenAny(tcs.Task, Task.Delay(20000));
            Assert.True(tcs.Task.IsCompleted);

            Assert.False(client.RemoveSubscription("not-exist"));

            client.Stop();
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }

    [Trait("Category","ClientValidation")]
    [Fact]
    public async Task Start_Stop_Toggles_IsRunning_And_ServerVersion_Set()
    {
        int port = PortAllocator.GetFreeTcpPort();
        var server = new SocketServer(port, false);
        server.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server);
        try
        {
            var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "CV-Run");
            Assert.False(client.IsRunning);
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) tcs.TrySetResult(true); };
            client.Start();
            // Allow background worker to flip IsRunning
            var startedAt = DateTime.UtcNow;
            while (!client.IsRunning && (DateTime.UtcNow - startedAt).TotalSeconds < 2)
                await Task.Delay(50);
            await Task.WhenAny(tcs.Task, Task.Delay(20000));
            Assert.True(tcs.Task.IsCompleted);
            Assert.True(client.ServerVersion >= 0); // ServerVersion available post-handshake (non-negative)
            client.Stop();
            Assert.False(client.IsRunning);
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }
}

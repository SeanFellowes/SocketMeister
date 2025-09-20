using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class CompressionTests
{
    private static async Task<SocketClient> CreateAndConnectAsync(int port, bool compress, string name)
    {
        var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, compress, name);
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.ConnectionAttemptFailed += (s, e) => { try { Console.WriteLine($"ConnectionAttemptFailed[{name}]: {e.Reason} {e.Message} ep={e.EndPoint?.Description}"); } catch { } };
        client.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) tcs.TrySetResult(true); };
        client.Start();
        await Task.WhenAny(tcs.Task, Task.Delay(60000));
        Assert.True(tcs.Task.IsCompleted, "Client did not connect");
        return client;
    }

    [Trait("Category","Compression")]
    [Fact]
    public async Task Echo_With_Compression_On_Both_Sides()
    {
        var server = new SocketServer(0, CompressSentData: true);
        server.MessageReceived += (s, e) => { e.Response = Encoding.UTF8.GetBytes((string)e.Parameters[0]); };
        server.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server);
        await Task.Delay(200);
        int port = ServerTestHelpers.GetBoundPort(server);
        try
        {
            var client = await CreateAndConnectAsync(port, true, "CompEcho");
            var text = "HelloCompression";
            var reply = client.SendMessage(new object[] { text }, 5000);
            Assert.Equal(text, Encoding.UTF8.GetString(reply));
            client.Stop();
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }

    [Trait("Category","Compression")]
    [Fact]
    public async Task Echo_With_Compression_Server_Only()
    {
        var server = new SocketServer(0, CompressSentData: true);
        server.MessageReceived += (s, e) => { e.Response = Encoding.UTF8.GetBytes((string)e.Parameters[0]); };
        server.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server);
        await Task.Delay(200);
        int port = ServerTestHelpers.GetBoundPort(server);
        try
        {
            // client compression disabled
            var client = await CreateAndConnectAsync(port, false, "CompServerOnly");
            var text = "ServerOnly";
            var reply = client.SendMessage(new object[] { text }, 5000);
            Assert.Equal(text, Encoding.UTF8.GetString(reply));
            client.Stop();
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }

    [Trait("Category","Compression")]
    [Fact]
    public async Task Echo_With_Compression_Client_Only()
    {
        var server = new SocketServer(0, CompressSentData: false);
        server.MessageReceived += (s, e) => { e.Response = Encoding.UTF8.GetBytes((string)e.Parameters[0]); };
        server.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server);
        await Task.Delay(200);
        int port = ServerTestHelpers.GetBoundPort(server);
        try
        {
            // client compression enabled
            var client = await CreateAndConnectAsync(port, true, "CompClientOnly");
            var text = "ClientOnly";
            var reply = client.SendMessage(new object[] { text }, 5000);
            Assert.Equal(text, Encoding.UTF8.GetString(reply));
            client.Stop();
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }

    [Trait("Category","Compression")]
    [Fact]
    public async Task Large_Payload_Roundtrip_With_Compression()
    {
        var server = new SocketServer(0, CompressSentData: true);
        server.MessageReceived += (s, e) => { e.Response = Encoding.UTF8.GetBytes((string)e.Parameters[0]); };
        server.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server);
        await Task.Delay(200);
        int port = ServerTestHelpers.GetBoundPort(server);
        try
        {
            var client = await CreateAndConnectAsync(port, true, "CompLarge");
            var payload = new string('X', 100_000);
            var reply = client.SendMessage(new object[] { payload }, 10000);
            var text = Encoding.UTF8.GetString(reply);
            Assert.Equal(payload.Length, text.Length);
            client.Stop();
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }
}

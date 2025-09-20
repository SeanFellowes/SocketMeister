using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class TelemetryTests
{
#if SOCKETMEISTER_TELEMETRY
    [Trait("Category","Telemetry")]
    [Fact]
    public async Task Telemetry_Basic_Connection_And_Message_Counters_Move()
    {
        var server = new SocketServer(0, compressSentData: false);
        server.MessageReceived += (s, e) => { e.Response = Array.Empty<byte>(); };
        server.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server);
        int port = ServerTestHelpers.GetBoundPort(server);

        try
        {
            var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "TL-Conn");
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) tcs.TrySetResult(true); };
            client.Start();
            await Task.WhenAny(tcs.Task, Task.Delay(60000));
            Assert.True(tcs.Task.IsCompleted);

            // give aggregator at least one tick
            await Task.Delay(1000);

            var cs = client.GetSnapshot();
            var ss = server.GetSnapshot();

            Assert.True(cs.CurrentConnections >= 1);
            Assert.True(ss.CurrentConnections >= 1);

            long cm0 = cs.TotalMessages;
            long sm0 = ss.TotalMessages;

            // one round trip
            var resp = client.SendMessage(new object[]{ "ping" }, 3000, "telemetry-ping");
            Assert.NotNull(resp);

            // allow receive path to flush
            await Task.Delay(100);

            var cs2 = client.GetSnapshot();
            var ss2 = server.GetSnapshot();

            Assert.True(cs2.TotalMessages >= cm0 + 2); // client send + receive
            Assert.True(ss2.TotalMessages >= sm0 + 2); // server receive + send
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }

    [Trait("Category","Telemetry")]
    [Fact]
    public async Task Telemetry_Disable_Stops_Counter_Updates()
    {
        var server = new SocketServer(0, compressSentData: false);
        server.MessageReceived += (s, e) => { e.Response = Array.Empty<byte>(); };
        server.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server);
        int port = ServerTestHelpers.GetBoundPort(server);

        try
        {
            var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "TL-Off");
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) tcs.TrySetResult(true); };
            client.Start();
            await Task.WhenAny(tcs.Task, Task.Delay(60000));
            Assert.True(tcs.Task.IsCompleted);

            // baseline
            var b1 = client.GetSnapshot();

            // disable telemetry and perform a round trip
            client.TelemetryEnabled = false;
            var resp = client.SendMessage(new object[]{ "ping" }, 3000, "telemetry-ping");
            Assert.NotNull(resp);
            await Task.Delay(100);

            var b2 = client.GetSnapshot();

            // counters should not have advanced while disabled
            Assert.Equal(b1.TotalMessages, b2.TotalMessages);
            Assert.Equal(b1.TotalFailures, b2.TotalFailures);
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }
#endif
}


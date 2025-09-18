using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class ConnectionReasonTests
{
    [Trait("Category","Reasons")]
    [Fact(Skip = "Initial connection failures may not raise Disconnected events with reasons; library remains in Connecting. Keeping as a placeholder.")]
    public async Task ConnectionRefused_On_Closed_Port()
    {
        int closedPort = PortAllocator.GetFreeTcpPort(); // no server will bind
        var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", closedPort) }, false, "RefusedClient");

        var reasonTcs = new TaskCompletionSource<ClientDisconnectReason>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.ConnectionStatusChanged += (s, e) =>
        {
            if (e.NewStatus == SocketClient.ConnectionStatuses.Disconnected && e.Reason != ClientDisconnectReason.Unknown)
                reasonTcs.TrySetResult(e.Reason);
        };
        client.Start();

        await Task.WhenAny(reasonTcs.Task, Task.Delay(20000));
        Assert.True(reasonTcs.Task.IsCompleted, "Did not get a disconnected reason in time");
        Assert.Equal(ClientDisconnectReason.SocketConnectionRefused, reasonTcs.Task.Result);
        client.Stop();
    }

    [Trait("Category","Reasons")]
    [Fact]
    public async Task ClientIsStopping_Reason_On_Stop()
    {
        int port = PortAllocator.GetFreeTcpPort();
        var server = new SocketServer(port, false);
        server.Start();
        try
        {
            var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "StopReasonClient");
            var connected = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var stopped = new TaskCompletionSource<ClientDisconnectReason>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.ConnectionStatusChanged += (s, e) =>
            {
                if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) connected.TrySetResult(true);
                if (e.NewStatus == SocketClient.ConnectionStatuses.Disconnected) stopped.TrySetResult(e.Reason);
            };
            client.Start();
            await Task.WhenAny(connected.Task, Task.Delay(20000));
            Assert.True(connected.Task.IsCompleted, "Client did not connect");

            client.Stop();
            await Task.WhenAny(stopped.Task, Task.Delay(10000));
            Assert.True(stopped.Task.IsCompleted, "Did not observe disconnected after Stop()");
            Assert.Equal(ClientDisconnectReason.ClientIsStopping, stopped.Task.Result);
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }
}

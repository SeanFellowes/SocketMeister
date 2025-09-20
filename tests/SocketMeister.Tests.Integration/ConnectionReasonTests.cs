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
    [Fact]
    public async Task ConnectionRefused_On_Closed_Port()
    {
        int closedPort = PortAllocator.GetFreeTcpPort(); // no server will bind
        var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", closedPort) }, false, "RefusedClient");

        var reasonTcs = new TaskCompletionSource<ClientDisconnectReason>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.ConnectionAttemptFailed += (s, e) =>
        {
            if (e.Reason == ClientDisconnectReason.SocketConnectionRefused)
                reasonTcs.TrySetResult(e.Reason);
        };
        client.Start();

        var completed = await Task.WhenAny(reasonTcs.Task, Task.Delay(20000));
        Assert.Same(reasonTcs.Task, completed);
        var reason = await reasonTcs.Task;
        Assert.Equal(ClientDisconnectReason.SocketConnectionRefused, reason);
        client.Stop();
    }

    [Trait("Category","Reasons")]
    [Fact]
    public async Task ClientIsStopping_Reason_On_Stop()
    {
        var server = new SocketServer(0, false);
        server.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server);
        try
        {
            int port = ServerTestHelpers.GetBoundPort(server);
            var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "StopReasonClient");
            var connected = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var stopped = new TaskCompletionSource<ClientDisconnectReason>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.ConnectionStatusChanged += (s, e) =>
            {
                if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) connected.TrySetResult(true);
                if (e.NewStatus == SocketClient.ConnectionStatuses.Disconnected) stopped.TrySetResult(e.Reason);
            };
            client.Start();
            await Task.WhenAny(connected.Task, Task.Delay(60000));
            Assert.True(connected.Task.IsCompleted, "Client did not connect");

            client.Stop();
            var done = await Task.WhenAny(stopped.Task, Task.Delay(10000));
            Assert.Same(stopped.Task, done);
            var stoppedReason = await stopped.Task;
            Assert.Equal(ClientDisconnectReason.ClientIsStopping, stoppedReason);
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }
}

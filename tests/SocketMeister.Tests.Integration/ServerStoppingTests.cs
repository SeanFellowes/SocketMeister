using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class ServerStoppingTests
{
    [Trait("Category","Disconnect")]
    [Fact]
    public async Task Client_Raises_ServerStopping_On_Server_Stop()
    {
        int port = PortAllocator.GetFreeTcpPort();
        var server = new SocketServer(port, false);
        server.Start();
        try
        {
            var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "SS-Client");
            var connected = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var serverStopping = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var disconnectedWithReason = new TaskCompletionSource<SocketMeister.SocketClient.ConnectionStatusChangedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) connected.TrySetResult(true); };
            client.ServerStopping += (s, e) => serverStopping.TrySetResult(true);
            client.ConnectionStatusChanged += (s, e) =>
            {
                if (e.NewStatus == SocketClient.ConnectionStatuses.Disconnected)
                    disconnectedWithReason.TrySetResult(e);
            };
            client.Start();
            await Task.WhenAny(connected.Task, Task.Delay(10000));
            Assert.True(connected.Task.IsCompleted, "Client did not connect");

            server.Stop();
            var ssDone = await Task.WhenAny(serverStopping.Task, Task.Delay(5000));
            Assert.Same(serverStopping.Task, ssDone);

            // Disconnect reason should be ServerIsStopping
            var discDone = await Task.WhenAny(disconnectedWithReason.Task, Task.Delay(5000));
            Assert.Same(disconnectedWithReason.Task, discDone);
            var args = await disconnectedWithReason.Task;
            Assert.Equal(ClientDisconnectReason.ServerIsStopping, args.Reason);
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }
}

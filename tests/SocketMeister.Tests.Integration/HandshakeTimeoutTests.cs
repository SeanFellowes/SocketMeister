using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class HandshakeTimeoutTests
{
    [Trait("Category","Timeout")]
    [Fact]
    public async Task Handshake_Times_Out_When_Server_Does_Not_Respond()
    {
        int port = PortAllocator.GetFreeTcpPort();

        // Raw TCP listener that accepts but never sends Handshake1
        var listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        _ = Task.Run(async () =>
        {
            try
            {
                using var socket = await listener.AcceptSocketAsync().ConfigureAwait(false);
                // Do nothing; keep connection open so client enters handshake wait.
                await Task.Delay(TimeSpan.FromSeconds(40)).ConfigureAwait(false);
            }
            catch { }
        });

        var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "HS-Timeout");
        var disconnected = new TaskCompletionSource<ClientDisconnectReason>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.ConnectionStatusChanged += (s, e) =>
        {
            if (e.NewStatus == SocketClient.ConnectionStatuses.Disconnected)
                disconnected.TrySetResult(e.Reason);
        };

        try
        {
            client.Start();

            // Handshake timeout is ~30s; allow a bit more leeway
            var done = await Task.WhenAny(disconnected.Task, Task.Delay(TimeSpan.FromSeconds(45)));
            Assert.Same(disconnected.Task, done);
            var reason = await disconnected.Task;
            Assert.Equal(ClientDisconnectReason.HandshakeTimeout, reason);

            client.Stop();
        }
        finally
        {
            try { listener.Stop(); } catch { }
        }
    }
}

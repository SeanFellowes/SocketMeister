using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class RestartServerReconnectionTests
{
    [Trait("Category","Restart")]
    [Fact]
    public async Task Client_Reconnects_After_Server_Restart_And_Can_Send()
    {
        using var reservation = PortReservation.ReserveLoopbackPort();
        int port = reservation.Port;
        var server = new SocketServer(port, false);
        server.MessageReceived += (s, e) => { e.Response = Encoding.UTF8.GetBytes("ACK"); };
        reservation.Dispose();
        server.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server);
            await Task.Delay(200);
        try
        {
            var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "RestartClient");
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) tcs.TrySetResult(true); };
            client.Start();
            await Task.WhenAny(tcs.Task, Task.Delay(60000));
            Assert.True(tcs.Task.IsCompleted, "Client did not connect initially");

            // Confirm messaging works
            var r1 = client.SendMessage(new object[] { 1 }, 5000);
            Assert.Equal("ACK", Encoding.UTF8.GetString(r1));

            // Restart server
            server.Stop();
            await Task.Delay(1000);
            server.Start();
            await ServerTestHelpers.WaitForServerStartedAsync(server);
            await Task.Delay(200);

            // Wait for client to reconnect (event-driven) and send again
            var reconnected = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            EventHandler<SocketClient.ConnectionStatusChangedEventArgs> handler = null!;
            handler = (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) reconnected.TrySetResult(true); };
            client.ConnectionStatusChanged += handler;
            if (client.ConnectionStatus == SocketClient.ConnectionStatuses.Connected) reconnected.TrySetResult(true);
            await Task.WhenAny(reconnected.Task, Task.Delay(TimeSpan.FromSeconds(90)));
            client.ConnectionStatusChanged -= handler;
            Assert.True(reconnected.Task.IsCompleted, "Client did not reconnect after restart");

            var r2 = client.SendMessage(new object[] { 2 }, 5000);
            Assert.Equal("ACK", Encoding.UTF8.GetString(r2));

            client.Stop();
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }
}


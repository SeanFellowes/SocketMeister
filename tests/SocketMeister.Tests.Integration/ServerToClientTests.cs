using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class ServerToClientTests
{
    [Trait("Category","ServerToClient")]
    [Fact]
    public async Task Server_Sends_Message_To_Client_And_Many()
    {
        int port = PortAllocator.GetFreeTcpPort();
        var server = new SocketServer(port, false);
        var connectedClients = new List<SocketServer.Client>();
        var evAll = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        server.ClientConnected += (s, e) =>
        {
            lock (connectedClients)
            {
                connectedClients.Add(e.Client);
                if (connectedClients.Count >= 3) evAll.TrySetResult(true);
            }
        };
        server.Start();
        try
        {
            // Clients reply to server-initiated message via MessageReceived
            SocketClient NewClient(string name)
            {
                var c = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, name);
                c.MessageReceived += (s, e) =>
                {
                    // Echo back a response identifying the client
                    e.Response = Encoding.UTF8.GetBytes(name + ":" + (e.Parameters?[0]?.ToString() ?? ""));
                };
                return c;
            }

            var c1 = NewClient("C1");
            var c2 = NewClient("C2");
            var c3 = NewClient("C3");

            var tcs1 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var tcs2 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var tcs3 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            c1.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) tcs1.TrySetResult(true); };
            c2.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) tcs2.TrySetResult(true); };
            c3.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) tcs3.TrySetResult(true); };
            c1.Start(); c2.Start(); c3.Start();

            await Task.WhenAll(Task.WhenAny(tcs1.Task, Task.Delay(10000)),
                               Task.WhenAny(tcs2.Task, Task.Delay(10000)),
                               Task.WhenAny(tcs3.Task, Task.Delay(10000)));
            Assert.True(tcs1.Task.IsCompleted && tcs2.Task.IsCompleted && tcs3.Task.IsCompleted, "Clients did not connect");

            // Wait until server captured all 3
            await Task.WhenAny(evAll.Task, Task.Delay(TimeSpan.FromSeconds(10)));
            Assert.True(evAll.Task.IsCompleted, "Server did not record 3 client connections");

            // Send to one client
            var reply1 = connectedClients[0].SendMessage(new object[] { "hello" }, 5000);
            Assert.NotNull(reply1);
            var text1 = Encoding.UTF8.GetString(reply1);
            Assert.StartsWith("C", text1); // "C1:hello" etc

            // Send to many clients
            foreach (var rc in connectedClients)
            {
                var r = rc.SendMessage(new object[] { "ping" }, 5000);
                Assert.NotNull(r);
            }

            c1.Stop(); c2.Stop(); c3.Stop();
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class TimeoutTests
{
    [Trait("Category","Timeout")]
    [Fact]
    public async Task Client_SendMessage_TimesOut_When_Server_Delays()
    {
        int port = PortAllocator.GetFreeTcpPort();
        var server = new SocketServer(port, false);
        // Install handler that delays longer than client timeout
        server.MessageReceived += (s, e) =>
        {
            // Block synchronously to simulate long server processing
            System.Threading.Thread.Sleep(3000);
            e.Response = Encoding.UTF8.GetBytes("late");
        };
        server.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server);
        try
        {
            var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "TimeoutClient");
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) tcs.TrySetResult(true); };
            client.Start();
            await Task.WhenAny(tcs.Task, Task.Delay(30000));
            Assert.True(tcs.Task.IsCompleted, "Client did not connect");

            // 2s timeout, server waits 3s
            Assert.ThrowsAny<Exception>(() => client.SendMessage(new object[] { "long" }, TimeoutMilliseconds: 2000));

            client.Stop();
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }
}

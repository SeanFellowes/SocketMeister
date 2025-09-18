using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class ServerLoggingTests
{
    [Trait("Category","Logging")]
    [Fact]
    public async Task Server_Logs_Error_When_Handler_Throws()
    {
        int port = PortAllocator.GetFreeTcpPort();
        var server = new SocketServer(port, false);
        int logCount = 0;
        server.LogRaised += (s, e) => { logCount++; };
        server.MessageReceived += (s, e) => throw new InvalidOperationException("boom");
        server.Start();
        try
        {
            var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "LogClient");
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) tcs.TrySetResult(true); };
            client.Start();
            await Task.WhenAny(tcs.Task, Task.Delay(10000));
            Assert.True(tcs.Task.IsCompleted, "Client did not connect");

            // Cause server to execute handler and log exception
            try { client.SendMessage(new object[] { 1 }, 3000); } catch { }

            await Task.Delay(300);
            Assert.True(logCount > 0, "Expected server to raise LogRaised event");

            client.Stop();
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }
}

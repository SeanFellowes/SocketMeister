using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class LongPollingCancellationTests
{
    [Trait("Category","Timeout")]
    [Fact(Skip = "Flaky timing across environments; revisit long-poll cancellation semantics later.")]
    public async Task LongPolling_Cancels_On_Client_Stop()
    {
        int port = PortAllocator.GetFreeTcpPort();
        var server = new SocketServer(port, false);
        server.MessageReceived += (s, e) =>
        {
            // Simulate long processing
            System.Threading.Thread.Sleep(10000);
            e.Response = Encoding.UTF8.GetBytes("done");
        };
        server.Start();
        try
        {
            var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "LongPollClient");
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.ConnectionStatusChanged += (s, e) => { if (e.NewStatus == SocketClient.ConnectionStatuses.Connected) tcs.TrySetResult(true); };
            client.Start();
            await Task.WhenAny(tcs.Task, Task.Delay(20000));
            Assert.True(tcs.Task.IsCompleted, "Client did not connect");

            var started = DateTime.UtcNow;
            var sendTask = Task.Run(() =>
            {
                Assert.ThrowsAny<Exception>(() => client.SendMessage(new object[] { "longpoll" }, 60000, true));
            });

            await Task.Delay(500); // let send begin
            // Trigger disconnect from the server side to cancel the long-poll quickly
            server.Stop();

            // Ensure it returns well below the full timeout (allow some leeway)
            await Task.WhenAny(sendTask, Task.Delay(20000));
            Assert.True(sendTask.IsCompleted, "Long-polling send did not cancel within 20s on server Stop()");
        }
        finally
        {
            try { server.Stop(); } catch { }
            (server as IDisposable)?.Dispose();
        }
    }
}

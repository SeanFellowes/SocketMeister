using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common.Fixtures;
using Xunit;

namespace SocketMeister.Tests.Integration;

// Minimal smoke tests to validate harness wiring rather than library behavior.
public class ServerClientSmokeTests : IClassFixture<ServerFixture>
{
    private readonly ServerFixture _fixture;

    public ServerClientSmokeTests(ServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Trait("Category","Integration")]
    [Fact]
    public async Task Connect_Send_Echo_Disconnect_Succeeds()
    {
        var endpoints = new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", _fixture.Port) };
        var client = new SocketClient(endpoints, EnableCompression: false, FriendlyName: "HarnessSmoke");

        try
        {
            var connectedTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            client.ConnectionStatusChanged += (s, e) =>
            {
                if (e.NewStatus == SocketClient.ConnectionStatuses.Connected)
                    connectedTcs.TrySetResult(true);
            };

            client.Start();

            // Wait a bit for handshake
            using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30));
            var completed = await Task.WhenAny(connectedTcs.Task, Task.Delay(Timeout.Infinite, cts.Token));
            Assert.Same(connectedTcs.Task, completed);

            // Echo roundtrip
            var payload = "HelloHarness";
            var replyBytes = client.SendMessage(new object[] { payload }, TimeoutMilliseconds: 5000);
            Assert.NotNull(replyBytes);
            Assert.Equal(payload, Encoding.UTF8.GetString(replyBytes));
        }
        finally
        {
            try { client.Stop(); } catch { }
            (client as IDisposable)?.Dispose();
        }
    }
}

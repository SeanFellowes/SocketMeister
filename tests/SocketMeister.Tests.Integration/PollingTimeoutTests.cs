using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class PollingTimeoutTests
{
    [Trait("Category","Timeout")]
    [Fact]
    public async Task Polling_Timeout_Disconnects_And_Logs()
    {
        // Set up a client (not connected) so CurrentEndPoint exists
        int port = PortAllocator.GetFreeTcpPort();
        var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "Poll-Timeout");

        var disconnected = new TaskCompletionSource<ClientDisconnectReason>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.ConnectionStatusChanged += (s, e) =>
        {
            if (e.NewStatus == SocketClient.ConnectionStatuses.Disconnected)
                disconnected.TrySetResult(e.Reason);
        };

        // Set LastPollResponse far in the past via reflection
        var type = client.GetType();
        var prop = type.GetProperty("LastPollResponse", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(prop);
        prop!.SetValue(client, DateTime.UtcNow.AddSeconds(-10000));

        // Prepare stopwatch with elapsed >= polling frequency; simplest is to sleep 16s
        var sw = Stopwatch.StartNew();
        await Task.Delay(TimeSpan.FromSeconds(16));

        // Force InternalConnectionStatus = Connected so Disconnect path proceeds
        var statusProp = type.GetProperty("InternalConnectionStatus", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(statusProp);
        statusProp!.SetValue(client, Enum.Parse(statusProp.PropertyType, "Connected"));

        // Invoke private BgPerformPolling to trigger timeout logic synchronously
        var m = type.GetMethod("BgPerformPolling", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(m);
        m!.Invoke(client, new object[] { sw });

        await Task.WhenAny(disconnected.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.True(disconnected.Task.IsCompleted, "Polling timeout did not cause disconnect promptly");
        Assert.Equal(ClientDisconnectReason.PollingTimeout, disconnected.Task.Result);
    }
}

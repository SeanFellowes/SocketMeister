using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class ServerStatusTests
{
    [Trait("Category","ServerStatus")]
    [Fact]
    public async Task StatusChanged_Fired_On_Start_Stop()
    {
        int port = PortAllocator.GetFreeTcpPort();
        var server = new SocketServer(port, false);
        var seq = new List<SocketServerStatus>();
        server.StatusChanged += (s, e) => seq.Add(e.NewStatus);

        server.Start();
        await ServerTestHelpers.WaitForServerStartedAsync(server);
        await Task.Delay(100);
        server.Stop();

        Assert.Contains(SocketServerStatus.Starting, seq);
        Assert.Contains(SocketServerStatus.Started, seq);
        Assert.Contains(SocketServerStatus.Stopping, seq);
        Assert.Contains(SocketServerStatus.Stopped, seq);
    }

    [Trait("Category","ServerStatus")]
    [Fact]
    public void Invalid_Port_Throws()
    {
        Assert.Throws<ArgumentException>(() => new SocketServer(new SocketServerOptions { Port = -1 }));
        Assert.Throws<ArgumentException>(() => new SocketServer(new SocketServerOptions { Port = 70000 }));
    }
}

using System;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class ServerIdempotencyTests
{
    [Trait("Category","ServerLifecycle")]
    [Fact]
    public void Start_Twice_Throws_Stop_Idempotent()
    {
        int port = PortAllocator.GetFreeTcpPort();
        var server = new SocketServer(port, false);

        server.Start();
        Assert.Throws<InvalidOperationException>(() => server.Start());

        // First stop
        server.Stop();
        // Second stop should do nothing harmful
        server.Stop();
    }
}


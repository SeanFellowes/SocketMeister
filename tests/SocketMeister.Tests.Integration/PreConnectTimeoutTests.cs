using System;
using System.Collections.Generic;
using SocketMeister;
using SocketMeister.Tests.Common;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class PreConnectTimeoutTests
{
    [Trait("Category","Timeout")]
    [Fact]
    public void SendMessage_Before_Server_Start_Times_Out()
    {
        int port = PortAllocator.GetFreeTcpPort();
        var client = new SocketClient(new List<SocketEndPoint> { new SocketEndPoint("127.0.0.1", port) }, false, "PreConn");

        // Do not start server; sending must time out
        var ex = Assert.ThrowsAny<Exception>(() => client.SendMessage(new object[] { "ping" }, 1000));
        Assert.Contains("Timeout", ex.Message);
    }
}


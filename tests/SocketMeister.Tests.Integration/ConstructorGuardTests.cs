using System;
using System.Collections.Generic;
using SocketMeister;
using Xunit;

namespace SocketMeister.Tests.Integration;

public class ConstructorGuardTests
{
    [Trait("Category","CtorGuards")]
    [Fact]
    public void SocketClient_List_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SocketClient((List<SocketEndPoint>)null!, false, "x"));
    }

    [Trait("Category","CtorGuards")]
    [Fact]
    public void SocketClient_List_Empty_Throws()
    {
        Assert.Throws<ArgumentException>(() => new SocketClient(new List<SocketEndPoint>(), false, "x"));
    }

    [Trait("Category","CtorGuards")]
    [Fact]
    public void SocketEndPoint_Invalid_IP_Throws()
    {
        Assert.Throws<ArgumentException>(() => new SocketEndPoint(null, 5000));
        Assert.Throws<ArgumentException>(() => new SocketEndPoint("", 5000));
        Assert.Throws<ArgumentException>(() => new SocketEndPoint("not-an-ip", 5000));
    }

    [Trait("Category","CtorGuards")]
    [Fact]
    public void SocketEndPoint_Invalid_Port_Throws()
    {
        Assert.Throws<ArgumentException>(() => new SocketEndPoint("127.0.0.1", 0));
        Assert.Throws<ArgumentException>(() => new SocketEndPoint("127.0.0.1", 1023));
        Assert.Throws<ArgumentException>(() => new SocketEndPoint("127.0.0.1", 70000));
    }

    [Trait("Category","CtorGuards")]
    [Fact]
    public void SocketServer_Null_Options_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SocketServer((SocketServerOptions)null));
    }
}

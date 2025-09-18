using System;
using System.Net;
using System.Net.Sockets;

namespace SocketMeister.Tests.Common;

public static class PortAllocator
{
    public static int GetFreeTcpPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        try
        {
            listener.Start();
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            try { listener.Stop(); } catch { }
        }
    }
}


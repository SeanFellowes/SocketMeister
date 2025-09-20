using System;
using System.Net;
using System.Net.Sockets;

namespace SocketMeister.Tests.Common;

/// <summary>
/// Reserves a free TCP port on loopback by binding a listener and keeping it open
/// until disposed. This eliminates the race between "find free port" and
/// later server Start() on busy CI hosts.
/// </summary>
public sealed class PortReservation : IDisposable
{
    private TcpListener? _listener;

    public int Port { get; }

    private PortReservation(TcpListener listener, int port)
    {
        _listener = listener;
        Port = port;
    }

    /// <summary>
    /// Reserves an ephemeral port on 127.0.0.1 by binding to port 0 and keeping the
    /// listener open. Dispose the returned instance immediately before starting the server.
    /// </summary>
    public static PortReservation ReserveLoopbackPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        return new PortReservation(listener, port);
    }

    public void Dispose()
    {
        try { _listener?.Stop(); } catch { }
        _listener = null;
    }
}

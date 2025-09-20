using System;
using System.Net;
using System.Net.Sockets;

namespace SocketMeister.Tests.Common;

/// <summary>
/// Reserves a free TCP port on loopback by binding a socket (not listening)
/// and keeping it open until disposed. This eliminates the race between
/// "find free port" and later server Start() on busy CI hosts, while ensuring
/// that clients cannot accidentally connect to the reserved port.
/// </summary>
public sealed class PortReservation : IDisposable
{
    private Socket? _socket;

    public int Port { get; }

    private PortReservation(Socket socket, int port)
    {
        _socket = socket;
        Port = port;
    }

    /// <summary>
    /// Reserves an ephemeral port on 127.0.0.1 by binding to port 0 and keeping the
    /// socket bound (without calling Listen). Dispose immediately before starting the server.
    /// </summary>
    public static PortReservation ReserveLoopbackPort()
    {
        var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            ExclusiveAddressUse = true
        };
        sock.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        var port = ((IPEndPoint)sock.LocalEndPoint!).Port;
        return new PortReservation(sock, port);
    }

    public void Dispose()
    {
        try { _socket?.Dispose(); } catch { }
        _socket = null;
    }
}

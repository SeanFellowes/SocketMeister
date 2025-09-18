#if !NET35
using System.Net;

namespace SocketMeister
{
#if SMISPUBLIC
    /// <summary>
    /// Options for configuring <see cref="SocketServer"/> startup and behavior.
    /// </summary>
    public sealed class SocketServerOptions
#else
    internal sealed class SocketServerOptions
#endif
    {
        /// <summary>
        /// The local IP address to bind to. Defaults to 0.0.0.0 (all interfaces).
        /// </summary>
        public IPAddress BindAddress { get; set; } = IPAddress.Parse("0.0.0.0");

        /// <summary>
        /// The TCP port to listen on.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The listen backlog (pending connection queue length). Defaults to 500.
        /// </summary>
        public int Backlog { get; set; } = 500;

        /// <summary>
        /// Whether to compress payloads sent from the server.
        /// </summary>
        public bool CompressSentData { get; set; } = false;
    }
}
#endif

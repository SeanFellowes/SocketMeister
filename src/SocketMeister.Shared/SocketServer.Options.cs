#if !NET35
using System.Net;

namespace SocketMeister
{
#if SMISPUBLIC
    public sealed class SocketServerOptions
#else
    internal sealed class SocketServerOptions
#endif
    {
        public IPAddress BindAddress { get; set; } = IPAddress.Parse("0.0.0.0");
        public int Port { get; set; }
        public int Backlog { get; set; } = 500;
        public bool CompressSentData { get; set; } = false;
    }
}
#endif


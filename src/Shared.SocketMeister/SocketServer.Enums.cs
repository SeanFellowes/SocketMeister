#if !SILVERLIGHT && !SMNOSERVER

namespace SocketMeister
{
#if SMISPUBLIC
    public partial class SocketServer
#else
    internal partial class SocketServer
#endif
    {
    }
}
#endif
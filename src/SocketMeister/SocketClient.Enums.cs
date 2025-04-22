using System;

namespace SocketMeister
{
    /// <summary>
    /// An asynchronous, persistent TCP/IP socket client that supports multiple destinations.
    /// </summary>
#if SMISPUBLIC
    public partial class SocketClient : IDisposable
#else
        internal partial class SocketClient : IDisposable
#endif
    {
        /// <summary>
        /// Represents the connection status of a SocketClient to a SocketServer.
        /// </summary>
        public enum ConnectionStatuses
        {
            /// <summary>
            /// The socket is disconnected.
            /// </summary>
            Disconnected = 0,
            /// <summary>
            /// The socket is attempting to connect.
            /// </summary>
            Connecting = 1,
            /// <summary>
            /// The socket is connected.
            /// </summary>
            Connected = 2,
            /// <summary>
            /// The socket is disconnecting.
            /// </summary>
            Disconnecting = 3
        }
    }
}

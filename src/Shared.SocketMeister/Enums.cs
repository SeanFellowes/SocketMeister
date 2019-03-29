using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{
    /// <summary>
    /// A SocketClient's connection status to a SocketServer.
    /// </summary>
    public enum ConnectionStatuses
    {
        /// <summary>
        /// Socket is disconnected.
        /// </summary>
        Disconnected = 0,
        /// <summary>
        /// Socket is attempting to connect.
        /// </summary>
        Connecting = 1,
        /// <summary>
        /// Socket is connected.
        /// </summary>
        Connected = 2,
        /// <summary>
        /// Socket is disconnecting;
        /// </summary>
        Disconnecting = 3
    }


}


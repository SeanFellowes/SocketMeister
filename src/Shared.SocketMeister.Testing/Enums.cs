using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{
    internal enum TestStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Stopping = 2,
        Successful = 50,
        Failed = 100,
        Stopped = 200
    }

    /// <summary>
    /// A SocketClient's connection status to a SocketServer.
    /// </summary>
    internal enum TestHarnessClientConnectionStatus
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

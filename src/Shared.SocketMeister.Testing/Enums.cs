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
    /// THe status of the socket connection for orchestrating tests between HarnessController/ServerController and HarnessController/ClientController instances.
    /// </summary>
    internal enum SocketClientConnectionStatus
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


    /// <summary>
    /// Type of ControlBusClient
    /// </summary>
    internal enum ControlBusClientType
    {
        /// <summary>
        /// HarnessControlBusClient is for a ClientController
        /// </summary>
        ClientController = 0,
        /// <summary>
        /// HarnessControlBusClient is for a ServerController
        /// </summary>
        ServerController = 1
    }


    /// <summary>
    /// Type of socket server
    /// </summary>
    public enum SocketServerTypes
    {
        /// <summary>
        /// Standard socket server
        /// </summary>
        SocketServer = 0,
        /// <summary>
        /// Policy server
        /// </summary>
        PolicyServer = 1
    }

}

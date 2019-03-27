using System;
using System.IO;
using System.Net.Sockets;


namespace SocketMeister.Messages
{
    /// <summary>
    /// A basic message
    /// </summary>
    internal partial class Message
    {
        private SocketServer.Client _remoteClient = null;

        /// <summary>
        /// Only populated for messages received on the server. This is the remote client which sent the message
        /// </summary>
        internal SocketServer.Client RemoteClient
        {
            get { lock (_lock) { return _remoteClient; } }
            set { lock (_lock) { _remoteClient = value; } }
        }
    }

}

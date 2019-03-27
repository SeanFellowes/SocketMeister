using System;
using System.IO;
using System.Net.Sockets;


namespace SocketMeister.Messages
{
    internal partial class RequestMessage
    {
        private SocketServer.Client _remoteClient = null;

        /// <summary>
        /// The remote client which sent this RequestMessage (value null on SocketClient)
        /// </summary>
        internal SocketServer.Client RemoteClient
        {
            get { lock (_lock) { return _remoteClient; } }
            set { lock (_lock) { _remoteClient = value; } }
        }
    }

}

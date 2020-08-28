using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using SocketMeister.Testing;


namespace SocketMeister.Testing
{
    public partial class ServerController
    {
        private SocketServer.Client _listenerClient = null;

        /// Socketmeister client (from the server perspective)
        /// </summary>
        public SocketServer.Client ListenerClient
        {
            get { lock (Lock) { return _listenerClient; } }
            set { lock (Lock) { _listenerClient = value; } }
        }

        public ControlBusClientType ClientType { get { return  ControlBusClientType.ServerController; } }

        /// <summary>
        /// Lock to provide threadsafe operations
        /// </summary>
        public object Lock { get { return _lock; } }


        /// <summary>
        /// Sends a message 
        /// </summary>
        public void Disconnect()
        {
            object[] parms = new object[2];
            parms[0] = ControlMessage.ExitClient;
            ListenerClient.SendMessage(parms);

            //  Wait zzzz miniseconds for the client to send a ClientDisconnecting message.
        }

    }
}

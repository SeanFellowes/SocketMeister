using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using SocketMeister.Testing;


namespace SocketMeister.Testing
{
    public class HarnessServerController : ServerController
    {
        private SocketServer.Client _listenerClient = null;


        public HarnessServerController(int Port, int ControlBusClientId, string ControlBusServerIPAddress) : base(Port, ControlBusClientId, ControlBusServerIPAddress)
        {
        }

        /// Socketmeister client (from the server perspective)
        /// </summary>
        public SocketServer.Client ListenerClient
        {
            get { lock (Lock) { return _listenerClient; } }
            set { lock (Lock) { _listenerClient = value; } }
        }

        public ControlBusClientType ClientType { get { return  ControlBusClientType.ServerController; } }

        /// <summary>
        /// Sends a message 
        /// </summary>
        public void Disconnect()
        {
            object[] parms = new object[2];
            parms[0] = ControlBus.ControlMessage.ExitClient;
            ListenerClient.SendMessage(parms);

            //  Wait zzzz miniseconds for the client to send a ClientDisconnecting message.
        }

    }
}

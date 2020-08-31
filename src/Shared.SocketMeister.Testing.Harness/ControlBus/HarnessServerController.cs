using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using SocketMeister.Testing;


namespace SocketMeister.Testing.ControlBus
{
    public class HarnessServerController : ServerController, IDisposable
    {
        private bool _disposed = false;
        private bool _disposeCalled = false;
        private SocketServer.Client _controlBuslistenerClient = null;

        public HarnessServerController(int Port, int ControlBusClientId, string ControlBusServerIPAddress) : base(Port, ControlBusClientId, ControlBusServerIPAddress)
        {
        }

        public new void Dispose()
        {
            _disposeCalled = true;
            base.Dispose(true);
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected new virtual void Dispose(bool disposing)
        {
            if (_disposed == true || _disposeCalled == true) return;
            if (disposing)
            {
                _disposeCalled = true;
                _controlBuslistenerClient = null;
                base.Dispose(disposing);
                _disposed = true;
            }
        }


        public ControlBusClientType ClientType { get { return  ControlBusClientType.ServerController; } }

        /// <summary>
        /// Socketmeister client (from the server perspective)
        /// </summary>
        public SocketServer.Client ControlBusListenerClient
        {
            get { lock (Lock) { return _controlBuslistenerClient; } }
            set { lock (Lock) { _controlBuslistenerClient = value; } }
        }

        /// <summary>
        /// Sends a message 
        /// </summary>
        public void Disconnect()
        {
            object[] parms = new object[2];
            parms[0] = ControlBus.ControlMessage.ExitClient;
            ControlBusListenerClient.SendMessage(parms);

            //  Wait zzzz miniseconds for the client to send a ClientDisconnecting message.
        }

    }
}

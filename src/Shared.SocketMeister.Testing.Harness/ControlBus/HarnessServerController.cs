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
        private SocketServer.Client _listenerClient = null;

        public HarnessServerController(int Port, int ControlBusClientId, string ControlBusServerIPAddress) : base(Port, ControlBusClientId, ControlBusServerIPAddress)
        {
            //DateTime maxWait = DateTime.Now.AddMilliseconds(Constants.MaxWaitMsForControlBusClientToHarnessControllerConnect);
            //while (_disposeCalled == false && ListenerClient == null)
            //{
            //    if (DateTime.Now > maxWait) throw new TimeoutException("Timout waiting for ControlBus connection to be established");
            //}
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
            if (_disposed == true) return;
            if (disposing)
            {
                _disposeCalled = true;
                base.Dispose(disposing);
                _disposed = true;
            }
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

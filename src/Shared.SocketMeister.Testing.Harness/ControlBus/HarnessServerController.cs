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
        private readonly commands _commands = null;
        private bool _disposed = false;
        private bool _disposeCalled = false;
        private SocketServer.Client _controlBuslistenerClient = null;

        public HarnessServerController(int Port, int ControlBusClientId, string ControlBusServerIPAddress) : base(Port, ControlBusClientId, ControlBusServerIPAddress)
        {
            _commands = new commands();
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


        public ControlBusClientType ClientType { get { return ControlBusClientType.ServerController; } }

        public commands Commands {  get { return _commands; } }

        /// <summary>
        /// Socketmeister client (from the server perspective)
        /// </summary>
        public SocketServer.Client ControlBusListenerClient
        {
            get { lock (Lock) { return _controlBuslistenerClient; } }
            set
            {
                lock (Lock)
                {
                    _controlBuslistenerClient = value;
                }
                _commands.ControlBusListenerClient = value;
            }
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

        public class commands
        {
            private SocketServer.Client _controlBuslistenerClient = null;
            private readonly object _lock = new object();

            public commands()
            {
            }

            public SocketServer.Client ControlBusListenerClient
            {
                get 
                { 
                    lock (_lock) 
                    {
                        if (_controlBuslistenerClient == null)
                            throw new NullReferenceException( "Function failed. Property " + nameof(ControlBusListenerClient) + " is null.");
                        return _controlBuslistenerClient; 
                    } 
                }
                set { lock (_lock) { _controlBuslistenerClient = value; } }
            }

            public void StartSocketServer(int Port)
            {
                object[] parms = new object[2];
                parms[0] = ControlMessage.SocketServerStart;
                parms[1] = Port;
                ControlBusListenerClient.SendRequest(parms);
                string ergerg = "";
            }











        }

    }
}

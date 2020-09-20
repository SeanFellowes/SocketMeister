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
        private readonly ControlBusCommands _commands = null;
        private bool _disposed = false;
        private bool _disposeCalled = false;
        private SocketServer.Client _controlBuslistenerClient = null;

        public HarnessServerController(int ControlBusClientId, string ControlBusServerIPAddress) : base(ControlBusClientId, ControlBusServerIPAddress)
        {
            _commands = new ControlBusCommands();
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

        public ControlBusCommands Commands {  get { return _commands; } }

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



        public class ControlBusCommands
        {
            private SocketServer.Client _controlBuslistenerClient = null;
            private readonly object _lock = new object();

            public ControlBusCommands()
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


            public byte[] ExecuteMethod(string ClassName, string StaticMethodName, object[] Parameters = null)
            {
                if (Parameters != null)
                {
                    object[] parms = new object[4];
                    parms[0] = ControlMessage.ExecuteMethod;
                    parms[1] = ClassName;
                    parms[2] = StaticMethodName;
                    parms[3] = Serializer.SerializeParameters(Parameters);
                    return ControlBusListenerClient.SendRequest(parms);
                }
                else
                {
                    object[] parms = new object[3];
                    parms[0] = ControlMessage.ExecuteMethod;
                    parms[1] = ClassName;
                    parms[2] = StaticMethodName;
                    return ControlBusListenerClient.SendRequest(parms);
                }
            }



            public void SocketServerStart(int Port)
            {
                object[] parms = new object[2];
                parms[0] = ControlMessage.SocketServerStart;
                parms[1] = Port;
                ControlBusListenerClient.SendRequest(parms);
            }


            public void SocketServerStop()
            {
                object[] parms = new object[1];
                parms[0] = ControlMessage.SocketServerStop;
                ControlBusListenerClient.SendRequest(parms);
            }









        }

    }
}

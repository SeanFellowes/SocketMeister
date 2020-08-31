using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace SocketMeister.Testing.ControlBus
{
    /// <summary>
    /// Controls a socket server
    /// </summary>
    public class ServerController : IDisposable
    {
        private readonly ControlBus.ControlBusClient _controlBusClient;
        private bool _disposed = false;
        private bool _disposeCalled = false;
        private readonly object _lock = new object();
        private readonly int _port;
        private SocketServer _socketServer = null;

        /// <summary>
        /// Triggered when connection could not be established, or failed, with the HarnessController. This ServerController should now abort (close)
        /// </summary> 
        public event EventHandler<EventArgs> ControlBusConnectionFailed;

        /// <summary>
        /// Raised when the status of the socket listener changes.
        /// </summary>
        public event EventHandler<SocketServer.SocketServerStatusChangedEventArgs> ListenerStateChanged;

        /// <summary>
        /// Raised when an trace log event has been raised.
        /// </summary>
        internal event EventHandler<TraceEventArgs> TraceEventRaised;

        /// <summary>
        /// Raised when a message is received from a client.
        /// </summary>
        internal event EventHandler<SocketServer.MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Raised when a request message is received from a client. A response can be provided which will be returned to the client.
        /// </summary>
        internal event EventHandler<SocketServer.RequestReceivedEventArgs> RequestReceived;

        /// <summary>
        /// Event raised when when there is a change to the clients connected to the socket server
        /// </summary>
        internal event EventHandler<SocketServer.ClientsChangedEventArgs> ClientsChanged;


        public ServerController(int Port, int ControlBusClientId, string ControlBusServerIPAddress)
        {
            //  CONNECT TO THE HarnessController
            _controlBusClient = new ControlBusClient(ControlBusClientType.ClientController, ControlBusClientId, ControlBusServerIPAddress, Constants.ControlBusPort);
            _controlBusClient.ConnectionFailed += ControlBus_ConnectionFailed;
            _controlBusClient.ControlBusSocketClient.MessageReceived += ControlBus_MessageReceived;

            _port = Port;
        }

        /// <summary>
        /// Lock to provide threadsafe operations
        /// </summary>
        public object Lock { get { return _lock; } }





        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (_disposed == true) return;
            if (disposing)
            {
                _disposeCalled = true;
                StopSocketServer();
                _socketServer = null;
                _disposed = true;
            }
        }




        private void ControlBus_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            int r = Convert.ToInt32(e.Parameters[0]);
            if (r == ControlBus.ControlMessage.SocketServerStart)
            {
                int Port = Convert.ToInt32(e.Parameters[1]);

                //  THIS WORKS. 
                
                //  START THE SOCKET SERVER ON THE PORT REQUESTED SEND AND ACKNOWLEDGEMENT SOMETHING BACK
            }
        }

        private void ControlBus_ConnectionFailed(object sender, EventArgs e)
        {
            //  CONNECTION TO THE HarnessController COULD NOT BE ESTABLISHED. PARENT FORM (IF THERE IS ONE) SHOULD CLOSE
            ControlBusConnectionFailed?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopAll()
        {
            _controlBusClient.Stop();
            StopSocketServer();
        }


        public int ClientId { get { return _controlBusClient.ControlBusClientId; } }

        public SocketServer SocketServer
        {
            get { return _socketServer; }
        }

        public int Port
        {
            get { return _port; }
        }


        private void SocketServer_ListenerStateChanged(object sender, SocketServer.SocketServerStatusChangedEventArgs e)
        {
            ListenerStateChanged?.Invoke(this, e);
        }


        internal void StartSocketServer()
        {
            //  START SOCKET SERVER IN BACHGROUND
            Thread bgStartSocketServer = new Thread(new ThreadStart(delegate
            {
                _socketServer = new SocketServer(_port, true);
                _socketServer.ClientsChanged += SocketServer_ClientsChanged;
                _socketServer.ListenerStateChanged += SocketServer_ListenerStateChanged;
                _socketServer.TraceEventRaised += SocketServer_TraceEventRaised;
                _socketServer.MessageReceived += SocketServer_MessageReceived;
                _socketServer.RequestReceived += SocketServer_RequestReceived;
                _socketServer.Start();
            }));
            bgStartSocketServer.IsBackground = true;
            bgStartSocketServer.Start();
        }

        internal void StopSocketServer()
        {
            if (_socketServer == null) return;

            //  UNREGISTER EVENTS
            _socketServer.ClientsChanged -= SocketServer_ClientsChanged;
            _socketServer.ListenerStateChanged -= SocketServer_ListenerStateChanged;
            _socketServer.TraceEventRaised -= SocketServer_TraceEventRaised;
            _socketServer.MessageReceived -= SocketServer_MessageReceived;
            _socketServer.RequestReceived -= SocketServer_RequestReceived;

            //  STOP SOCKET SERVER
            if (_socketServer.Status == SocketServerStatus.Started)
            {
                _socketServer.Stop();
            }
        }

        private void SocketServer_MessageReceived(object sender, SocketServer.MessageReceivedEventArgs e)
        {
            MessageReceived?.Invoke(sender, e);
        }

        private void SocketServer_RequestReceived(object sender, SocketServer.RequestReceivedEventArgs e)
        {
            RequestReceived?.Invoke(sender, e);
        }

        private void SocketServer_ClientsChanged(object sender, SocketServer.ClientsChangedEventArgs e)
        {
            ClientsChanged?.Invoke(sender, e);
        }

        private void SocketServer_TraceEventRaised(object sender, TraceEventArgs e)
        {
            TraceEventRaised?.Invoke(sender, e);
        }



    }
}

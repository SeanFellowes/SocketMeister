using SocketMeister.Testing.ControlBus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace SocketMeister.Testing
{
    /// <summary>
    /// Controls a socket server
    /// </summary>
    internal class ServerController : IDisposable
    {
        private readonly ControlBusClient _controlBusClient;
        private bool _disposed;
        private bool _disposeCalled;
        private readonly object _lock = new object();
        private readonly static OpenTransactions _openTransactions = new OpenTransactions();
        private int _port;
        private SocketServer _socketServer = null;

        /// <summary>
        /// Triggered when connection could not be established, or failed, with the HarnessController. This ServerController should now abort (close)
        /// </summary> 
        public event EventHandler<EventArgs> ControlBusConnectionFailed;

        /// Event raised when an exception occurs
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ExceptionRaised;

        /// <summary>
        /// Raised when the status of the socket listener changes.
        /// </summary>
        public event EventHandler<EventArgs> ListenerStateChanged;

        /// <summary>
        /// Raised when an trace log event has been raised.
        /// </summary>
        internal event EventHandler<TraceEventArgs> TraceEventRaised;

        /// <summary>
        /// Raised when a request message is received from a client. A response can be provided which will be returned to the client.
        /// </summary>
        internal event EventHandler<SocketServer.RequestReceivedEventArgs> RequestReceived;

        /// <summary>
        /// Event raised when when there is a change to the clients connected to the socket server
        /// </summary>
        internal event EventHandler<SocketServer.ClientEventArgs> ClientsChanged;


        public ServerController(int ControlBusClientId, string ControlBusServerIPAddress)
        {
            //  CONNECT TO THE HarnessController
            _controlBusClient = new ControlBusClient(ControlBusClientType.ClientController, ControlBusClientId, ControlBusServerIPAddress, Constants.ControlBusPort);
            _controlBusClient.ConnectionFailed += ControlBusClient_ConnectionFailed;
            _controlBusClient.MessageReceived += ControlBusClient_MessageReceived;
            _controlBusClient.RequestReceived += ControlBusClient_RequestReceived;
            _controlBusClient.ExceptionRaised += ControlBusClient_ExceptionRaised;
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
            if (_disposed == true || _disposeCalled == true) return;
            if (disposing)
            {
                _disposeCalled = true;
                StopSocketServer();
                _socketServer = null;
                _disposed = true;
            }
        }


        internal OpenTransactions OpenMessages {  get { return _openTransactions; } }




        private void ControlBusClient_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            //int r = Convert.ToInt32(e.Parameters[0]);

            //if (r == ControlBus.ControlMessage.SocketServerStart)
            //{
            //    int Port = Convert.ToInt32(e.Parameters[1]);

            //    //  THIS WORKS. 
                
            //    //  START THE SOCKET SERVER ON THE PORT REQUESTED SEND AND ACKNOWLEDGEMENT SOMETHING BACK
            //}
        }

        private void ControlBusClient_RequestReceived(object sender, SocketClient.RequestReceivedEventArgs e)
        {
            short messageType = (short)e.Parameters[0];
            if (messageType == ControlMessage.SocketServerStart)
            {
                int Port = Convert.ToInt32(e.Parameters[1]);

                StopSocketServer();
                StartSocketServer(Port);
            }
            else if (messageType == ControlMessage.SocketServerStop)
            {
                StopSocketServer();
            }

            else
                throw new ArgumentOutOfRangeException(nameof(e) + "." + nameof(e.Parameters) + "[0]", "No process defined for " + nameof(e) + "." + nameof(e.Parameters) + "[0] = " + messageType + ".");
        }


        private void ControlBusClient_ConnectionFailed(object sender, EventArgs e)
        {
            //  CONNECTION TO THE HarnessController COULD NOT BE ESTABLISHED. PARENT FORM (IF THERE IS ONE) SHOULD CLOSE
            ControlBusConnectionFailed?.Invoke(this, e);
        }

        private void ControlBusClient_ExceptionRaised(object sender, ExceptionEventArgs e)
        {
            ExceptionRaised?.Invoke(this, e);
        }


        public void Start()
        {
            _controlBusClient.Start();
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
            get { lock (_lock) { return _port; } }
            set {  lock(_lock) { _port = value; } }
        }



        private void SocketServer_StatusChanged(object sender, EventArgs e)
        {
            ListenerStateChanged?.Invoke(this, e);
        }


        internal void StartSocketServer(int Port)
        {
            ////  START SOCKET SERVER IN BACHGROUND
            //Thread bgStartSocketServer = new Thread(new ThreadStart(delegate
            //{
            this.Port = Port;
            _openTransactions.Clear();
            _socketServer = new SocketServer(Port, true);
            _socketServer.ClientConnected += SocketServer_ClientsChanged;
            _socketServer.ClientDisconnected += SocketServer_ClientsChanged;
            _socketServer.StatusChanged += SocketServer_StatusChanged;
            _socketServer.TraceEventRaised += SocketServer_TraceEventRaised;
            _socketServer.RequestReceived += SocketServer_RequestReceived;
            _socketServer.Start();
            //}));
            //bgStartSocketServer.IsBackground = true;
            //bgStartSocketServer.Start();
        }

        internal void StopSocketServer()
        {
            if (_socketServer == null) return;

            //  UNREGISTER EVENTS
            _socketServer.ClientConnected -= SocketServer_ClientsChanged;
            _socketServer.ClientDisconnected -= SocketServer_ClientsChanged;
            _socketServer.StatusChanged -= SocketServer_StatusChanged;
            _socketServer.TraceEventRaised -= SocketServer_TraceEventRaised;
            _socketServer.RequestReceived -= SocketServer_RequestReceived;

            //  STOP SOCKET SERVER
            if (_socketServer.Status == SocketServerStatus.Started)
            {
                _socketServer.Stop();
            }
            _openTransactions.Clear();
        }

        private void SocketServer_RequestReceived(object sender, SocketServer.RequestReceivedEventArgs e)
        {
            int TransactionId = (int)e.Parameters[0];
            OpenTransaction trans = _openTransactions[TransactionId];
            if (trans != null)
            {
                trans.UserToken = e.Parameters;
                trans.IsTransacted = true;
                _openTransactions.Remove(trans);
            }

            //  RAISE EVENT
            RequestReceived?.Invoke(sender, e);
        }

        private void SocketServer_ClientsChanged(object sender, SocketServer.ClientEventArgs e)
        {
            ClientsChanged?.Invoke(sender, e);
        }

        private void SocketServer_TraceEventRaised(object sender, TraceEventArgs e)
        {
            TraceEventRaised?.Invoke(sender, e);
        }



    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SocketMeister.Testing.ControlBus
{
    /// <summary>
    /// Socket server for the control bus. This runs on the test harness.
    /// </summary>
    internal class ControlBusServer : IDisposable
    {
        private readonly SocketServer _listener = null;

        /// <summary>
        /// Raised when the status of the socket listener changes.
        /// </summary>
        public event EventHandler<EventArgs> StatusChanged;

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


        public ControlBusServer()
        {
            _listener = new SocketServer(Constants.ControlBusPort, true);
            _listener.ClientConnected += Listener_ClientsChanged;
            _listener.ClientDisconnected += Listener_ClientsChanged;
            _listener.StatusChanged += Listener_StatusChanged;
            _listener.TraceEventRaised += Listener_TraceEventRaised;
            _listener.RequestReceived += Listener_RequestReceived;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
            }
        }

        public SocketServer Listener
        {
            get { return _listener; }
        }

        public int Port
        {
            get { return Constants.ControlBusPort; }
        }

        internal void Start()
        {
           _listener.Start();
        }

        internal void Stop()
        {
            //  UNREGISTER EVENTS
            _listener.ClientConnected -= Listener_ClientsChanged;
            _listener.ClientDisconnected -= Listener_ClientsChanged;
            _listener.StatusChanged -= Listener_StatusChanged;
            _listener.TraceEventRaised -= Listener_TraceEventRaised;
            _listener.RequestReceived -= Listener_RequestReceived;

            //  STOP SOCKET SERVER
            if (_listener.Status == SocketServerStatus.Started)
            {
                _listener.Stop();
            }
        }


        private void Listener_StatusChanged(object sender, EventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }


        private void Listener_RequestReceived(object sender, SocketServer.RequestReceivedEventArgs e)
        {
            RequestReceived?.Invoke(sender, e);
        }

        private void Listener_ClientsChanged(object sender, SocketServer.ClientEventArgs e)
        {
            ClientsChanged?.Invoke(sender, e);
        }

        private void Listener_TraceEventRaised(object sender, TraceEventArgs e)
        {
            TraceEventRaised?.Invoke(sender, e);
        }
    }
}

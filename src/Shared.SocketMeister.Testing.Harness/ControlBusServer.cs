using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SocketMeister.Testing
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


        public ControlBusServer()
        {
            _listener = new SocketServer(Constants.ControlBusPort, true);
            _listener.ClientsChanged += Listener_ClientsChanged;
            _listener.ListenerStateChanged += Listener_ListenerStateChanged;
            _listener.TraceEventRaised += Listener_TraceEventRaised;
            _listener.MessageReceived += Listener_MessageReceived;
            _listener.RequestReceived += Listener_RequestReceived;
            _listener.Start();
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

        internal void Stop()
        {
            if (_listener == null) return;

            //  UNREGISTER EVENTS
            _listener.ClientsChanged -= Listener_ClientsChanged;
            _listener.ListenerStateChanged -= Listener_ListenerStateChanged;
            _listener.TraceEventRaised -= Listener_TraceEventRaised;
            _listener.MessageReceived -= Listener_MessageReceived;
            _listener.RequestReceived -= Listener_RequestReceived;

            //  STOP SOCKET SERVER
            if (_listener.Status == SocketServerStatus.Started)
            {
                _listener.Stop();
            }
        }


        private void Listener_ListenerStateChanged(object sender, SocketServer.SocketServerStatusChangedEventArgs e)
        {
            ListenerStateChanged?.Invoke(this, e);
        }


        private void Listener_MessageReceived(object sender, SocketServer.MessageReceivedEventArgs e)
        {
            MessageReceived?.Invoke(sender, e);
        }

        private void Listener_RequestReceived(object sender, SocketServer.RequestReceivedEventArgs e)
        {
            RequestReceived?.Invoke(sender, e);
        }

        private void Listener_ClientsChanged(object sender, SocketServer.ClientsChangedEventArgs e)
        {
            ClientsChanged?.Invoke(sender, e);
        }

        private void Listener_TraceEventRaised(object sender, TraceEventArgs e)
        {
            TraceEventRaised?.Invoke(sender, e);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SocketMeister.Testing
{
    /// <summary>
    /// Socket server for the control bus. This runs on the test harness.
    /// </summary>
    internal class ControlBusServer
    {
        private readonly SocketServer _socketServer = null;

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
            _socketServer = new SocketServer(Constants.ControlBusPort, true);
            _socketServer.ClientsChanged += SocketServer_ClientsChanged;
            _socketServer.ListenerStateChanged += SocketServer_ListenerStateChanged;
            _socketServer.TraceEventRaised += SocketServer_TraceEventRaised;
            _socketServer.MessageReceived += SocketServer_MessageReceived;
            _socketServer.RequestReceived += SocketServer_RequestReceived;
            _socketServer.Start();
        }


        public SocketServer SocketServer
        {
            get { return _socketServer; }
        }

        public int Port
        {
            get { return Constants.ControlBusPort; }
        }


        private void SocketServer_ListenerStateChanged(object sender, SocketServer.SocketServerStatusChangedEventArgs e)
        {
            ListenerStateChanged?.Invoke(this, e);
        }


        internal void Stop()
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

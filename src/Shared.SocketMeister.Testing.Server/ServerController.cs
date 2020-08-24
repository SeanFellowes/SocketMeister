using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SocketMeister.Testing
{
    /// <summary>
    /// Controls a socket server
    /// </summary>
    public class ServerController
    {
        private readonly object _lock = new object();
        private readonly int _port;
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


        public ServerController(int Port)
        {
            _port = Port;
            _socketServer = new SocketServer(Port, true);
            _socketServer.ClientsChanged += SocketServer_ClientsChanged;
            _socketServer.ListenerStateChanged += SocketServer_ListenerStateChanged;
            _socketServer.TraceEventRaised += SocketServer_TraceEventRaised;
            _socketServer.MessageReceived += SocketServer_MessageReceived;
            _socketServer.RequestReceived += SocketServer_RequestReceived;
        }


        /// <summary>
        /// When parent form is closing, unregister events to stop errors occuring when socket shutdown events bubble up to the non-existent parent
        /// </summary>
        public void UnregisterEvents()
        {
            if (_socketServer != null)
            {
                _socketServer.ClientsChanged -= SocketServer_ClientsChanged;
                _socketServer.ListenerStateChanged -= SocketServer_ListenerStateChanged;
                _socketServer.TraceEventRaised -= SocketServer_TraceEventRaised;
                _socketServer.MessageReceived -= SocketServer_MessageReceived;
                _socketServer.RequestReceived -= SocketServer_RequestReceived;
            }
        }

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



        /// <summary>
        /// Attempts to stop the service.
        /// </summary>
        /// <returns>True if an attempt to stop occured, False is the service is not running</returns>
        public bool Stop()
        {
                if (_socketServer == null) return false;
                if (_socketServer.Status == SocketServerStatus.Started)
                {
                    _socketServer.Stop();
                    return true;
                }
                return false;
        }


        public void Start()
        {
            //  START IN THE BACKGROUND
            BackgroundWorker bgStartService = new BackgroundWorker();
            bgStartService.DoWork += BgStartService_DoWork;
            bgStartService.RunWorkerAsync();
        }


        private void BgStartService_DoWork(object sender, DoWorkEventArgs e)
        {
            _socketServer.Start();
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace SocketMeister.Testing
{
    /// <summary>
    /// Test Harness Client
    /// </summary>
    internal class ClientController : IDisposable
    {
        private readonly ControlBus.ControlBusClient _controlBusClient;
        private readonly object _lock = new object();
        private SocketClient _socketClient = null;

        /// <summary>
        /// Triggered when connection could not be established with the HarnessController. This ClientController should now abort (close)
        /// </summary> 
        public event EventHandler<EventArgs> ControlBusConnectionFailed;

        /// <summary>
        /// Event raised when a status of a socket connection has changed
        /// </summary>
        public event EventHandler<SocketClient.ConnectionStatusChangedEventArgs> ControlBusConnectionStatusChanged;


        public ClientController(int ControlBusClientId)
        {
            _controlBusClient = new ControlBus.ControlBusClient( ControlBusClientType.ClientController, ControlBusClientId, Constants.ControlBusServerIPAddress, Constants.ControlBusPort);
            _controlBusClient.ConnectionFailed += ControlBusClient_ConnectionFailed;
            _controlBusClient.ConnectionStatusChanged += ControlBusClient_ConnectionStatusChanged;
            _controlBusClient.ControlBusSocketClient.MessageReceived += ControlBusClient_MessageReceived; ;
            _controlBusClient.ControlBusSocketClient.RequestReceived += ControlBusClient_RequestReceived; ;
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


        public int ClientId {  get { return _controlBusClient.ControlBusClientId; } }

        /// <summary>
        /// Lock to provide threadsafe operations
        /// </summary>
        public object Lock { get { return _lock; } }

        public SocketClient SocketClient { get {  lock(_lock) { return _socketClient; } } }



        private void ControlBusClient_RequestReceived(object sender, SocketClient.RequestReceivedEventArgs e)
        {
            int messageType = Convert.ToInt32(e.Parameters[0]);

            if (messageType == ControlBus.ControlMessage.SocketClientStart)
            {
                List<SocketEndPoint> endPoints = null;
                bool enableCompression;

                using (MemoryStream stream = new MemoryStream((byte[])e.Parameters[1]))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    int capacity = reader.ReadInt32();
                    endPoints = new List<SocketEndPoint>(capacity);
                    for (int ptr = 1; ptr <= capacity; ptr++)
                    {
                        endPoints.Add(new SocketEndPoint(reader.ReadString(), reader.ReadUInt16()));
                    }
                    enableCompression = reader.ReadBoolean();
                }
                SocketClientStart(endPoints, enableCompression);
            }

            else if (messageType == ControlBus.ControlMessage.SocketClientStop)
            {
                SocketClientStop();
            }
        }

        private void ControlBusClient_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ControlBusClient_ConnectionFailed(object sender, EventArgs e)
        {
            //  CONNECTION TO THE HarnessController COULD NOT BE ESTABLISHED.
            ControlBusConnectionFailed?.Invoke(this, e);
        }

        private void ControlBusClient_ConnectionStatusChanged(object sender, SocketClient.ConnectionStatusChangedEventArgs e)
        {
            ControlBusConnectionStatusChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// Attempts to cleanly disconnect the control bus client
        /// </summary>
        public void Stop()
        {
            _controlBusClient.Stop();
        }



        internal void SocketClientStart(List<SocketEndPoint> EndPoints, bool EnableCompression)
        {
            lock (_lock)
            {
                 SocketClientStop();
                _socketClient = new SocketClient(EndPoints, EnableCompression);
            }
        }


        internal void SocketClientStop()
        {
            lock (_lock)
            {
                if (_socketClient == null) return;
                _socketClient.Stop();
                _socketClient.Dispose();
                _socketClient = null;
            }

        }

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            _controlBusClient.ControlBusSocketClient.MessageReceived += ControlBusClient_MessageReceived;
        }

        public int ClientId {  get { return _controlBusClient.ControlBusClientId; } }

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


        /// <summary>
        /// Lock to provide threadsafe operations
        /// </summary>
        public object Lock { get { return _lock; } }



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

    }
}

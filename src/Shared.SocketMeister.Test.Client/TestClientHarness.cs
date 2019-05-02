using System;
using System.Collections.Generic;
using System.Text;
using SocketMeister.Testing;

namespace SocketMeister
{
    public class TestClientHarness
    {
        private readonly SocketClient controlSocket = null;

        /// <summary>
        /// Event raised when a status of a socket connection has changed
        /// </summary>
        public event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;


        public TestClientHarness()
        {
            List<SocketEndPoint> endPoints = new List<SocketEndPoint>() { new SocketEndPoint("127.0.0.1", 4505) };
            controlSocket = new SocketClient(endPoints);
            controlSocket.ConnectionStatusChanged += controlSocket_ConnectionStatusChanged;
        }


        /// <summary>
        /// The connection status of the socket client
        /// </summary>
        public ConnectionStatus ConnectionStatus
        {
            get { return (ConnectionStatus)controlSocket.ConnectionStatus; } 
        }


        private void controlSocket_ConnectionStatusChanged(object sender, SocketClient.ConnectionStatusChangedEventArgs e)
        {
            ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs((ConnectionStatus)e.Status, e.IPAddress, e.Port));
        }

        public void Stop()
        {
            controlSocket.Stop();
        }
    }
}

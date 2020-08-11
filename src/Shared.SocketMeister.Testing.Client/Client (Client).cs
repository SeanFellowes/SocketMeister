using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using SocketMeister.Testing;


namespace SocketMeister.Testing
{
    /// <summary>
    /// Test Harness Client (CLIENT SIDE)
    /// </summary>
    internal partial class Client
    {
        private readonly SocketClient controlSocket = null;
        private readonly DispatcherTimer controlConnectedTimer = null;

        /// <summary>
        /// Event raised when a status of a socket connection has changed
        /// </summary>
        public event EventHandler<TestHarnessClientConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        /// <summary>
        /// Triggered when connection the the control socket failed or could not start.
        /// </summary> 
        public event EventHandler<EventArgs> ControlConnectionFailed;


        public Client(int ClientId)
        {
            this.ClientId = ClientId;

            controlConnectedTimer = new DispatcherTimer();
            controlConnectedTimer.Interval = new TimeSpan(0, 0, 10);
            controlConnectedTimer.Tick += ControlConnectedTimer_Tick;
            controlConnectedTimer.Start();

            //  CONNECT TO THE TEST SERVER ON THE CONTROL CHANNEL AT PORT 4505. THIS WILL RECEIVE INSTRUCTIONS FROM THE TEST SERVER
            List<SocketEndPoint> endPoints = new List<SocketEndPoint>() { new SocketEndPoint("127.0.0.1", 4505) };
            controlSocket = new SocketClient(endPoints, true);
            controlSocket.ConnectionStatusChanged += ControlSocket_ConnectionStatusChanged;
            controlSocket.MessageReceived += ControlSocket_MessageReceived;

        }

        private void ControlSocket_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// The connection status of the socket client
        /// </summary>
        public TestHarnessClientConnectionStatus ConnectionStatus
        {
            get { return (TestHarnessClientConnectionStatus)controlSocket.ConnectionStatus; }
        }


        private void ControlConnectedTimer_Tick(object sender, EventArgs e)
        {
            controlConnectedTimer.Stop();
            if (controlSocket == null || controlSocket.ConnectionStatus != SocketClient.ConnectionStatuses.Connected)
            {
                try
                {
                    if (controlSocket != null) controlSocket.Stop();
                }
                catch { }
                ControlConnectionFailed?.Invoke(this, new EventArgs());
            }
            else
            {
                controlConnectedTimer.Start();
            }
        }


        private void ControlSocket_ConnectionStatusChanged(object sender, SocketClient.ConnectionStatusChangedEventArgs e)
        {
            //  SEND A CONTROL MESSAGE TO THE SERVER
            if (e.Status == SocketClient.ConnectionStatuses.Connected)
            {
                object[] parms = new object[2];
                parms[0] = ControlMessage.ClientConnected;
                parms[1] = ClientId;
                controlSocket.SendRequest(parms);
            }
            ConnectionStatusChanged?.Invoke(this, new TestHarnessClientConnectionStatusChangedEventArgs((TestHarnessClientConnectionStatus)e.Status, e.IPAddress, e.Port));
        }


        public void Stop()
        {
            controlSocket.Stop();
        }



    }
}

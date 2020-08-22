using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using SocketMeister.Testing;


namespace SocketMeister.Testing
{
    /// <summary>
    /// Test Harness Client (CLIENT SIDE)
    /// </summary>
    internal partial class ClientController
    {
        private readonly SocketClient controlSocket = null;

        /// <summary>
        /// Event raised when a status of a socket connection has changed
        /// </summary>
        public event EventHandler<TestHarnessClientConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        /// <summary>
        /// Triggered when connection the the control socket failed or could not start.
        /// </summary> 
        public event EventHandler<EventArgs> ControlConnectionFailed;


        public ClientController(int ClientId, string HarnessControllerIPAddress, int HarnessControllerPort)
        {
            _clientId = ClientId;

            Thread bgFailIfDisconnected = new Thread(new ThreadStart(delegate
            {
                DateTime waitUntil = DateTime.Now.AddMilliseconds(5000);
                while (true == true)
                {
                    if (DateTime.Now > waitUntil && ( controlSocket == null || controlSocket.ConnectionStatus != SocketClient.ConnectionStatuses.Connected))
                    {
                        if (controlSocket != null) controlSocket.Stop();
                        ControlConnectionFailed?.Invoke(this, new EventArgs());
                        break;
                    }
                    Thread.Sleep(250);
                }
            }));
            bgFailIfDisconnected.IsBackground = true;
            bgFailIfDisconnected.Start();

            //  CONNECT TO THE TEST SERVER ON THE CONTROL CHANNEL AT PORT 4505. THIS WILL RECEIVE INSTRUCTIONS FROM THE TEST SERVER
            List<SocketEndPoint> endPoints = new List<SocketEndPoint>() { new SocketEndPoint(HarnessControllerIPAddress, HarnessControllerPort) };
            controlSocket = new SocketClient(endPoints, true);
            controlSocket.ConnectionStatusChanged += ControlSocket_ConnectionStatusChanged;
            controlSocket.MessageReceived += ControlSocket_MessageReceived;

        }

        private void bg()
        {

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


        private void ControlSocket_ConnectionStatusChanged(object sender, SocketClient.ConnectionStatusChangedEventArgs e)
        {
            //  SEND A CONTROL MESSAGE TO THE SERVER
            if (e.Status == SocketClient.ConnectionStatuses.Connected)
            {
                object[] parms = new object[2];
                parms[0] = ControlMessage.ClientControllerConnected;
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

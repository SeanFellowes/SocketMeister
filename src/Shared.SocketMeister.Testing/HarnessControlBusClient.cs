using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SocketMeister.Testing
{
    /// <summary>
    /// Socket client for sending and receiving control messages between a ClientController and HarnessController or ServerController and HarnessController
    /// </summary>
    internal class HarnessControlBusClient
    {
        private readonly int _harnessControlBusClientId;
        private readonly HarnessControlBusClientType _harnessControlBusClientType;
        private readonly string _harnessControlBusIPAddress;
        private readonly int _harnessControlBusPort;
        private readonly SocketClient _harnessControlBusSocketClient = null;

        private readonly object _lock = new object();

        /// <summary>
        /// Event raised when a status of a socket connection has changed
        /// </summary>
        public event EventHandler<SocketClient.ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        /// <summary>
        /// Triggered when connection the the control socket failed or could not start. This should never happen. ServerController or ClientController should exit.
        /// </summary> 
        public event EventHandler<EventArgs> ConnectionFailed;

        public HarnessControlBusClient(HarnessControlBusClientType HarnessControlBusClientType, int HarnessControlBusClientId, string HarnessControlBusIPAddress, int HarnessControlBusPort)
        {
            _harnessControlBusClientType = HarnessControlBusClientType;
            _harnessControlBusClientId = HarnessControlBusClientId;
            _harnessControlBusIPAddress = HarnessControlBusIPAddress;
            _harnessControlBusPort = HarnessControlBusPort;

            //  CONNECT TO THE TEST HARNESS ON THE CONTROL CHANNEL AT PORT Constants.HarnessControlBusPort. THE CONTROL CHANEL ALLOWS COMMUNICATION BACK AND FORTH FROM TEST HARNESS TO CLIENT
            List<SocketEndPoint> endPoints = new List<SocketEndPoint>() { new SocketEndPoint(HarnessControlBusIPAddress, HarnessControlBusPort) };
            _harnessControlBusSocketClient = new SocketClient(endPoints, true);
            _harnessControlBusSocketClient.ConnectionStatusChanged += TestHarnessControlBus_ConnectionStatusChanged;
            _harnessControlBusSocketClient.MessageReceived += TestHarnessControlBus_MessageReceived;

            Thread bgFailIfDisconnected = new Thread(new ThreadStart(delegate
            {
                //  WAIT UP TO 5 SECONDS FOR THE HarnessControlBusSocketClient TO CONNECT TO THE HarnessController
                DateTime maxWait = DateTime.Now.AddMilliseconds(5000);
                while (true == true)
                {
                    if (_harnessControlBusSocketClient.ConnectionStatus == SocketClient.ConnectionStatuses.Connected)
                    {
                        break;
                    }
                    else if (DateTime.Now > maxWait)
                    {
                        if (_harnessControlBusSocketClient != null) _harnessControlBusSocketClient.Stop();
                        ConnectionFailed?.Invoke(this, new EventArgs());
                        break;
                    }
                    else
                    {
                        Thread.Sleep(250);
                    }
                }
            }));
            bgFailIfDisconnected.IsBackground = true;
            bgFailIfDisconnected.Start();
        }

        public SocketClient HarnessControlBusSocketClient
        {
            get { return _harnessControlBusSocketClient; }
        }

        public int HarnessControlBusClientId
        {
            get { return _harnessControlBusClientId; } 
        }

        public HarnessControlBusClientType HarnessControlBusClientType
        {
            get { return _harnessControlBusClientType; }
        }

        public int HarnessControlBusPort
        {
            get { return _harnessControlBusPort; } 
        }

        public string HarnessControlBusIPAddress
        {
            get { return _harnessControlBusIPAddress; }
        }


        private void TestHarnessControlBus_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// The connection status of the socket client
        /// </summary>
        public SocketClientConnectionStatus HarnessControlBusConnectionStatus
        {
            get { return (SocketClientConnectionStatus)_harnessControlBusSocketClient.ConnectionStatus; }
        }


        private void TestHarnessControlBus_ConnectionStatusChanged(object sender, SocketClient.ConnectionStatusChangedEventArgs e)
        {
            //  SEND A CONTROL MESSAGE TO THE SERVER
            if (e.Status == SocketMeister.SocketClient.ConnectionStatuses.Connected)
            {
                object[] parms = new object[2];
                parms[0] = ControlMessage.HarnessControlBusConnecting;
                parms[1] = HarnessControlBusClientId;
                _harnessControlBusSocketClient.SendRequest(parms);
            }
            ConnectionStatusChanged?.Invoke(this, e);
        }


        public void Stop()
        {
            _harnessControlBusSocketClient.Stop();
        }
    }
}

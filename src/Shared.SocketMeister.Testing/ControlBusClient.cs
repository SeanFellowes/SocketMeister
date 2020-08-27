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
    internal class ControlBusClient
    {
        private readonly int _controlBusClientId;
        private readonly ControlBusClientType _controlBusClientType;
        private readonly int _controlBusPort;
        private readonly string _controlBusServerIPAddress;
        private readonly SocketClient _controlBusSocketClient = null;

        private readonly object _lock = new object();

        /// <summary>
        /// Event raised when a status of a socket connection has changed
        /// </summary>
        public event EventHandler<SocketClient.ConnectionStatusChangedEventArgs> ConnectionStatusChanged;

        /// <summary>
        /// Triggered when connection the the control socket failed or could not start. This should never happen. ServerController or ClientController should exit.
        /// </summary> 
        public event EventHandler<EventArgs> ConnectionFailed;

        public ControlBusClient(ControlBusClientType ControlBusClientType, int ControlBusClientId, string ControlBusServerIPAddress, int ControlBusPort)
        {
            _controlBusClientType = ControlBusClientType;
            _controlBusClientId = ControlBusClientId;
            _controlBusServerIPAddress = ControlBusServerIPAddress;
            _controlBusPort = ControlBusPort;

            //  CONNECT TO THE TEST HARNESS ON THE CONTROL CHANNEL AT PORT Constants.HarnessControlBusPort. THE CONTROL CHANEL ALLOWS COMMUNICATION BACK AND FORTH FROM TEST HARNESS TO CLIENT
            List<SocketEndPoint> endPoints = new List<SocketEndPoint>() { new SocketEndPoint(ControlBusServerIPAddress, ControlBusPort) };
            _controlBusSocketClient = new SocketClient(endPoints, true);
            _controlBusSocketClient.ConnectionStatusChanged += ControlBus_ConnectionStatusChanged;
            _controlBusSocketClient.MessageReceived += ControlBus_MessageReceived;

            Thread bgFailIfDisconnected = new Thread(new ThreadStart(delegate
            {
                //  WAIT UP TO 5 SECONDS FOR THE HarnessControlBusSocketClient TO CONNECT TO THE HarnessController
                DateTime maxWait = DateTime.Now.AddMilliseconds(5000);
                while (true == true)
                {
                    if (_controlBusSocketClient.ConnectionStatus == SocketClient.ConnectionStatuses.Connected)
                    {
                        break;
                    }
                    else if (DateTime.Now > maxWait)
                    {
                        if (_controlBusSocketClient != null) _controlBusSocketClient.Stop();
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

        public SocketClient ControlBusSocketClient
        {
            get { return _controlBusSocketClient; }
        }

        public int ControlBusClientId
        {
            get { return _controlBusClientId; } 
        }

        public ControlBusClientType ControlBusClientType
        {
            get { return _controlBusClientType; }
        }

        public int ControlBusPort
        {
            get { return _controlBusPort; } 
        }

        public string ControlBusServerIPAddress
        {
            get { return _controlBusServerIPAddress; }
        }


        private void ControlBus_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// The connection status of the socket client
        /// </summary>
        public SocketClientConnectionStatus SocketClientConnectionStatus
        {
            get { return (SocketClientConnectionStatus)_controlBusSocketClient.ConnectionStatus; }
        }


        private void ControlBus_ConnectionStatusChanged(object sender, SocketClient.ConnectionStatusChangedEventArgs e)
        {
            //  SEND A CONTROL MESSAGE TO THE SERVER
            if (e.Status == SocketClient.ConnectionStatuses.Connected)
            {
                object[] parms = new object[2];
                parms[0] = ControlMessage.HarnessControlBusClientIsConnecting;
                parms[1] = ControlBusClientId;
                _controlBusSocketClient.SendRequest(parms);
            }
            ConnectionStatusChanged?.Invoke(this, e);
        }


        public void Stop()
        {
            _controlBusSocketClient.Stop();
        }
    }
}

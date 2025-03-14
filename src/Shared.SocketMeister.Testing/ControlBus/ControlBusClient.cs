﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace SocketMeister.Testing.ControlBus
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
        private SocketClient _controlBusSocketClient = null;
        private readonly object _lock = new object();


        /// <summary>
        /// Event raised when a status of a socket connection has changed
        /// </summary>
        public event EventHandler<EventArgs> ConnectionStatusChanged;

        /// <summary>
        /// Triggered when connection the the control socket failed or could not start. This should never happen. ServerController or ClientController should exit.
        /// </summary> 
        public event EventHandler<EventArgs> ConnectionFailed;

        /// Event raised when an exception occurs
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ExceptionRaised;


        /// <summary>
        /// Raised when a request message is received from the server. A response can be provided which will be returned to the server.
        /// </summary>
        public event EventHandler<SocketClient.MessageReceivedEventArgs> MessageReceived;



        public ControlBusClient(ControlBusClientType ControlBusClientType, int ControlBusClientId, string ControlBusServerIPAddress, int ControlBusPort)
        {
            _controlBusClientType = ControlBusClientType;
            _controlBusClientId = ControlBusClientId;
            _controlBusServerIPAddress = ControlBusServerIPAddress;
            _controlBusPort = ControlBusPort;

        }

        private SocketClient ControlBusSocketClient
        {
            get
            {
                lock (_lock)
                {
                    if (_controlBusSocketClient == null) throw new InvalidOperationException("Method Start() must be successfully called first.");
                    return _controlBusSocketClient;
                }
            }
            set { lock (_lock) { _controlBusSocketClient = value; } }
        }

        public int ControlBusClientId => _controlBusClientId;

        public ControlBusClientType ControlBusClientType => _controlBusClientType;

        public int ControlBusPort => _controlBusPort;

        public string ControlBusServerIPAddress => _controlBusServerIPAddress;


        /// <summary>
        /// The connection status of the socket client
        /// </summary>
        public SocketClientConnectionStatus SocketClientConnectionStatus => (SocketClientConnectionStatus)ControlBusSocketClient.ConnectionStatus;



        public void Start()
        {
            //  CONNECT TO THE TEST HARNESS ON THE CONTROL CHANNEL AT PORT Constants.HarnessControlBusPort. THE CONTROL CHANEL ALLOWS COMMUNICATION BACK AND FORTH FROM TEST HARNESS TO CLIENT
            List<SocketEndPoint> endPoints = new List<SocketEndPoint>() { new SocketEndPoint(ControlBusServerIPAddress, ControlBusPort) };
            ControlBusSocketClient = new SocketClient(endPoints, true);
            ControlBusSocketClient.ConnectionStatusChanged += ControlBusSocketClient_ConnectionStatusChanged;
            ControlBusSocketClient.ExceptionRaised += ControlBusSocketClient_ExceptionRaised;
            ControlBusSocketClient.MessageReceived += ControlBusSocketClient_MessageReceived;

            //  WAIT UP TO 5 SECONDS FOR THE HarnessControlBusSocketClient TO CONNECT TO THE HarnessController
            DateTime maxWait = DateTime.UtcNow.AddMilliseconds(5000);

            while (true)
            {
                if (ControlBusSocketClient.ConnectionStatus == SocketClient.ConnectionStatuses.Connected)
                {
                    break;
                }
                else if (DateTime.UtcNow > maxWait)
                {
                    ControlBusSocketClient?.Stop();
                    ConnectionFailed?.Invoke(this, new EventArgs());
                    break;
                }
                else
                {
                    Thread.Sleep(250);
                }
            }
            //}));
            //bgFailIfDisconnected.IsBackground = true;
            //bgFailIfDisconnected.Start();
        }

        private void ControlBusSocketClient_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        private void ControlBusSocketClient_ExceptionRaised(object sender, ExceptionEventArgs e)
        {
            ExceptionRaised?.Invoke(this, e);
        }


        private void ControlBusSocketClient_ConnectionStatusChanged(object sender, EventArgs e)
        {
            try
            {
                //  SEND A CONTROL MESSAGE TO THE SERVER
                SocketClient client = (SocketClient)sender;
                if (client.ConnectionStatus == SocketClient.ConnectionStatuses.Connected)
                {
                    object[] parms = new object[2];
                    parms[0] = ControlMessage.HarnessControlBusClientIsConnecting;
                    parms[1] = ControlBusClientId;
                    ControlBusSocketClient.SendMessage(parms);
                }
                ConnectionStatusChanged?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                ExceptionRaised?.Invoke(this, new ExceptionEventArgs(ex, 12000));
                throw;
            }
        }


        public void Stop()
        {
            ControlBusSocketClient.Stop();
        }
    }
}

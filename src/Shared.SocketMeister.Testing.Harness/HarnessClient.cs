using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using SocketMeister.Testing;


namespace SocketMeister.Testing
{
    /// <summary>
    /// Test Harness Client (TEST HOST)
    /// </summary>
    internal class HarnessControlBusClientSocketClient
    {
        private int _clientId;
        private HarnessControlBusClientType _clientType;
        private SocketServer.Client _socketClient = null;
        private readonly object _lockClass = new object();

        public HarnessControlBusClientSocketClient(HarnessControlBusClientType ClientType, int ClientId)
        {
            _clientType = ClientType;
            _clientId = ClientId;
        }


        /// <summary>
        /// Socketmeister client (from the server perspective)
        /// </summary>
        public SocketServer.Client SocketClient
        {
            get { lock (LockClass) { return _socketClient; } }
            set { lock (LockClass) { _socketClient = value; } }
        }


        public int ClientId
        {
            get { lock (_lockClass) { return _clientId; } }
        }

        public HarnessControlBusClientType ClientType {  get { return _clientType; } }

        /// <summary>
        /// Lock to provide threadsafe operations
        /// </summary>
        public object LockClass { get { return _lockClass; } }



        /// <summary>
        /// Launches and instance of the test application and waits for it to connect a socket back so the harness can control it
        /// </summary>
        /// <param name="MaxWaitMilliseconds"></param>
        public void LaunchClientApplication(int MaxWaitMilliseconds = 5000)
        {
            Process process = new Process();
            process.StartInfo.FileName = @"SocketMeister.Test.Client.WinForms.exe";
            process.StartInfo.Arguments = ClientId.ToString();
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = true;
            process.Start();
            DateTime maxWait = DateTime.Now.AddMilliseconds(MaxWaitMilliseconds);
            while (DateTime.Now < maxWait)
            {
                if (process.HasExited == true)
                {
                    if (process.ExitCode == 1) throw new ApplicationException("Client failed to start. Missing ClientId from process arguments.");
                    else if (process.ExitCode == 3) throw new ApplicationException("Client failed to start. ClientId must be numeric. This is the first parameter");
                    else if (process.ExitCode == 2) throw new ApplicationException("Client failed to start. Couldn't connect to control port " + Constants.HarnessControlBusPort + " (Used to sent test instructions and results between test clients and the test server).");
                    else throw new ApplicationException("Client failed to start. Unknown reason.");
                }

                //  CHECK TO SEE IF THE CLIENT HAS CONNECTED
                if (_socketClient != null)
                {
                    //  CONNECTED
                    return;
                }

                Thread.Sleep(250);
            }
            throw new ApplicationException("Client did not connect within " + MaxWaitMilliseconds + " milliseconds");
        }


        /// <summary>
        /// Sends a message 
        /// </summary>
        public void Disconnect()
        {
            object[] parms = new object[2];
            parms[0] = ControlMessage.ExitClient;
            SocketClient.SendMessage(parms);

            //  Wait zzzz miniseconds for the client to send a ClientDisconnecting message.
        }



    }
}

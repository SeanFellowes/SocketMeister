using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using SocketMeister.Testing;

#if TESTHARNESS
using System.Management.Instrumentation;
#endif

namespace SocketMeister
{
    /// <summary>
    /// Test Harness Client
    /// </summary>
    internal class TestHarnessClient
    {
        private readonly int _clientId;
        private readonly object _lock = new object();

#if TESTHARNESS
        private readonly TestHarnessClientCollection _parentCollection;
        private SocketServer.Client _client = null;
#elif TESTHARNESSCLIENT
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
#endif


#if TESTHARNESS

        /// <summary>
        /// Default constructor. Should only be called from TestHarnessClientCollection. Automatically connects to the test harness control port (Port 4505)
        /// </summary>
        /// <param name="ParentCollection"></param>
        public TestHarnessClient(TestHarnessClientCollection ParentCollection)
        {
            _parentCollection = ParentCollection;
            _clientId = TestHarness.GetNextClientId();
        }
#elif TESTHARNESSCLIENT
        public TestHarnessClient(int ClientId)
        {
            _clientId = ClientId;

            controlConnectedTimer = new DispatcherTimer();
            controlConnectedTimer.Interval = new TimeSpan(0, 0, 10);
            controlConnectedTimer.Tick += ControlConnectedTimer_Tick;
            controlConnectedTimer.Start();

            //  CONNECT TO THE TEST SERVER ON THE CONTROL CHANNEL AT PORT 4505. THIS WILL RECEIVE INSTRUCTIONS FROM THE TEST SERVER
            List<SocketEndPoint> endPoints = new List<SocketEndPoint>() { new SocketEndPoint("127.0.0.1", 4505) };
            controlSocket = new SocketClient(endPoints, true);
            controlSocket.ConnectionStatusChanged += controlSocket_ConnectionStatusChanged;

        }
#endif

        public int ClientId {  get { return _clientId; } }


#if TESTHARNESS
        /// <summary>
        /// Socketmeister client (from the server perspective)
        /// </summary>
        public SocketServer.Client Client
        {
            get { lock (_lock) { return _client; } }
            set { lock (_lock) { _client = value; } }
        }

        /// <summary>
        /// TestHarnessClientCollection that this client belongs to
        /// </summary>
        public TestHarnessClientCollection ParentCollection {  get { return _parentCollection; } }
#elif TESTHARNESSCLIENT
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


        private void controlSocket_ConnectionStatusChanged(object sender, SocketClient.ConnectionStatusChangedEventArgs e)
        {
            //  SEND A CONTROL MESSAGE TO THE SERVER
            if (e.Status == SocketClient.ConnectionStatuses.Connected)
            {
                object[] parms = new object[2];
                parms[0] = ControlMessage.ClientConnected;
                parms[1] = _clientId;
                controlSocket.SendRequest(parms);
            }

            ConnectionStatusChanged?.Invoke(this, new TestHarnessClientConnectionStatusChangedEventArgs((TestHarnessClientConnectionStatus)e.Status, e.IPAddress, e.Port));
        }

#endif






#if TESTHARNESS

        public void Connect(int MaxWaitMilliseconds = 5000)
        {
            Process process = new Process();
            process.StartInfo.FileName = @"SocketMeister.Test.Client.WinForms.exe";
            process.StartInfo.Arguments = _clientId.ToString();
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = true;
            process.Start();
            DateTime maxWait = DateTime.Now.AddMilliseconds(MaxWaitMilliseconds);
            while (DateTime.Now < maxWait)
            {
                if (process.HasExited == true)
                {
                    maxWait = DateTime.Now.AddHours(-1);
                    if (process.ExitCode == 1) throw new ApplicationException("Client failed to start. Missing ClientId from process arguments.");
                    else if (process.ExitCode == 3) throw new ApplicationException("Client failed to start. ClientId must be numeric. This is the first parameter");
                    else if (process.ExitCode == 2) throw new ApplicationException("Client failed to start. Couldn't connect to control port 4505 (Used to sent test instructions and results between test clients and the test server).");
                    else throw new ApplicationException("Client failed to start. Unknown reason.");
                }

                //  CHECK TO SEE IF THE CLIENT HAS CONNECTED
                if (_client != null)
                {
                    //  CONNECTED
                    return;
                }

                Thread.Sleep(250);
            }
            throw new ApplicationException("Client did not connect within " + MaxWaitMilliseconds + " milliseconds");
        }

        public void Disconnect()
        {

        }
#elif TESTHARNESSCLIENT
        public void Stop()
        {
            controlSocket.Stop();
        }

#endif




    }
}

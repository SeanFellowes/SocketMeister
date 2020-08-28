using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SocketMeister.Testing.Tests;
using System.Threading;
using SocketMeister;
using System.ComponentModel;

namespace SocketMeister.Testing
{
    internal enum Executing
    {
        Stopped = 0,
        SingleTest = 1,
        AllTests = 2,
        StoppingAllTests = 20
    }

    internal class HarnessController : IDisposable
    {
        private readonly object classLock = new object();
        private readonly ControlBusServer _controlBusServer = null;
        private bool _disposed = false;
        private bool _disposeCalled = false;
        private HarnessClientCollection _testClientCollection = new HarnessClientCollection();
        private ITestOnHarness _currentTest = null;
        private Executing _executeMode = Executing.Stopped;
        private static readonly object _lock = new object();
        private readonly ClientController _fixedClient1;
        private readonly ServerController _fixedServer1;
        private readonly PolicyServer _policyServer;
        private readonly List<ITestOnHarness> _tests = new List<ITestOnHarness>();

        /// <summary>
        /// Raised when an trace log event has been raised.
        /// </summary>
        internal event EventHandler<TraceEventArgs> TraceEventRaised;

        /// <summary>
        /// The status of the socket server changed. Statuses include stating, started, stopping and stopped.
        /// </summary>
        public event EventHandler<SocketServer.SocketServerStatusChangedEventArgs> PolicyServerStatusChanged;




        public HarnessController()
        {
            //  DYNAMICALLY ADD TESTS. TESTS ARE ANY CLASS NAMED BETWEEN SocketMeister.Test001 and SocketMeister.Test999
            //  THIS ELIMINATES HAVING TO ADD CODE HERE WHEN A NEW TEST IS CREATED.
            //  IGNORE TEMPLATE TEST 000
            for (int ctr = 1; ctr <= 999; ctr++)
            {
                string className = typeof(Test000).Namespace + ".Test" + ctr.ToString("000", CultureInfo.InvariantCulture) + "Harness";
                Type t = Type.GetType(className);
                if (t != null) AddTest(t);
                if (t != null) AddTest(t);
                if (t != null) AddTest(t);
                if (t != null) AddTest(t);
                if (t != null) AddTest(t);
                if (t != null) AddTest(t);
                if (t != null) AddTest(t);
            }

            //  START CONTROL BUS LISTENER
            _controlBusServer = new ControlBusServer();
            _controlBusServer.RequestReceived += _controlBusServer_RequestReceived;

            //  SETUP POLICY SERVER
            _policyServer = new PolicyServer();
            _policyServer.ListenerStateChanged += PolicyServer_SocketServiceStatusChanged;
            _policyServer.TraceEventRaised += PolicyServer_TraceEventRaised;

            //  SETUP FIXED SERVER
            _fixedServer1 = new ServerController(Constants.HarnessFixedServerPort, int.MaxValue - 1, "127.0.0.1");

            //  SEAN SEAN SEAN 
            //  CREATE ServerController PROPERTY AND ALLOW USER CONTROL TO ATTACH TO COLLECT USERS CONNECTED (SEPERATE FROM Controller USERS)
            //  START THE ServerController (WITH A CONTROL MESSAGE)
            //  ADD STOP (WITH CONTROL MESSAGE) WHEN THIS HarnessCOntroller STOPS


            //  SETUP FIXED CLIENT
            _fixedClient1 = new ClientController(int.MaxValue, "127.0.0.1");
//#if !DEBUG
//            _fixedClientControllerHarnessClient.LaunchClientApplication();
//#endif


            new Thread(delegate ()
            {
                DateTime maxWait = DateTime.Now.AddSeconds(30);
                while (_disposeCalled == false)
                {
                    if (DateTime.Now > maxWait)
                        throw new TimeoutException("Visual Studio debug mode timed out waiting for the fixed client to connect. Make sure both the harness and client applications are set as Startup Projects.");
                    else if (_fixedClient1.ListenerClient != null)
                        break;
                    else
                        Thread.Sleep(1000);
                }
            }).Start();
        }


        private void _controlBusServer_RequestReceived(object sender, SocketServer.RequestReceivedEventArgs e)
        {
            int r = Convert.ToInt32(e.Parameters[0]);
            if (r == ControlMessage.HarnessControlBusClientIsConnecting)
            {
                int ClientId = Convert.ToInt32(e.Parameters[1]);
                if (ClientId == int.MaxValue)
                {
                    //  FIXED CLIENT HAS PHONED HOME
                    _fixedClient1.ListenerClient = e.Client;
                }
                else
                {
                    //  ANOTHER CLIENT HAS PHONED HOME. FIND THE CLIENT
                    ClientController client = _testClientCollection[ClientId];
                    if (client != null)
                    {
                        //  ASSIGN THE SocketMeister Server Client to the class. When connecting a test harness client, this value is checked for NOT null (Connected).
                        client.ListenerClient = e.Client;
                    }
                }
            }


        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (_disposed == true) return;
            if (disposing)
            {
                _disposeCalled = true;
                if (_fixedClient1 != null)
                {
                    try
                    {
                        _fixedClient1.Stop();
                    }
                    catch { }
                }

                if (_controlBusServer != null)
                {
                    try
                    {
                        _controlBusServer.Stop();
                        _controlBusServer.Dispose();
                    }
                    catch { }
                }

                if (_policyServer != null)
                {
                    try
                    {
                        if (_policyServer.ListenerState == SocketServerStatus.Started)
                        {
                            _policyServer.Stop();
                        }
                        _policyServer.ListenerStateChanged -= PolicyServer_SocketServiceStatusChanged;
                        _policyServer.TraceEventRaised -= PolicyServer_TraceEventRaised;
                        _policyServer.Dispose();
                    }
                    catch { }
                }

                if (_fixedServer1 != null)
                {
                    try
                    {
                        _fixedServer1.Dispose();
                    }
                    catch { }
                }


            }
        }

        public ControlBusServer ControlBusServer {  get { return _controlBusServer; } }

        public ServerController FixedServer1 {  get { return _fixedServer1; } }


        //SocketServer ControlBusSocketServer
        //{
        //    get { }
        //}

        private void AddTest(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            //object[] parms = new object[1];
            //parms[0] = ctr;
            //T test = (T)Activator.CreateInstance(t, parms);

            ITestOnHarness test = (ITestOnHarness)Activator.CreateInstance(t);
            _tests.Add(test);
        }


        private void PolicyServer_TraceEventRaised(object sender, TraceEventArgs e)
        {
            TraceEventRaised?.Invoke(this, e);
        }

        private void PolicyServer_SocketServiceStatusChanged(object sender, SocketServer.SocketServerStatusChangedEventArgs e)
        {
            PolicyServerStatusChanged?.Invoke(sender, e);
        }


        public void Start()
        {
                _policyServer.Start();

                ////  START IN THE BACKGROUND
                //BackgroundWorker bgStartService = new BackgroundWorker();
                //bgStartService.DoWork += BgStartService_DoWork;
                //bgStartService.RunWorkerCompleted += BgStartService_RunWorkerCompleted;
                //bgStartService.RunWorkerAsync();
        }

        //private void BgStartService_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    if (serverType == ServerType.SocketServer)
        //    {
        //        socketServer = new SocketServer(port, true);
        //        socketServer.ClientsChanged += SocketServer_ClientsChanged;
        //        socketServer.ListenerStateChanged += SocketServer_ListenerStateChanged;
        //        socketServer.TraceEventRaised += SocketServer_TraceEventRaised;
        //        socketServer.MessageReceived += SocketServer_MessageReceived;
        //        socketServer.RequestReceived += SocketServer_RequestReceived;
        //        socketServer.Start();
        //    }
        //    else
        //    {
        //    }
        //}

        //private void BgStartService_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    if (InvokeRequired) Invoke(new MethodInvoker(delegate { BgStartService_RunWorkerCompleted(sender, e); }));
        //    else
        //    {
        //        this.Cursor = Cursors.Default;
        //        if (e.Error != null)
        //        {
        //            string msg = e.Error.Message;
        //            if (e.Error.StackTrace != null) msg += "\n\n" + e.Error.StackTrace;
        //            MessageBox.Show(msg, "Error Starting", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //        else if (e.Cancelled == true)
        //        {
        //        }
        //    }
        //}



        //public void Initialize()
        //{

        //}

        /// <summary>
        /// Test Harness Client Collection used during testing.
        /// </summary>
        public HarnessClientCollection TestClientCollection { get { return _testClientCollection; } }

        /// <summary>
        /// Policy Server for Silverlight Clients
        /// </summary>
        public PolicyServer PolicyServer {  get { return _policyServer; } }

        /// <summary>
        /// Suite of tests which are available;
        /// </summary>
        public List<ITestOnHarness> Tests { get { return _tests; } }


        public ITestOnHarness CurrentTest
        {
            get { lock (classLock) { return _currentTest; } }
            private set 
            { 
                lock (classLock) 
                {
                    if (_currentTest == value) return;
                    _currentTest = value; 
                } 
            }
        }

        /// <summary>
        /// In DEBUG, this is attached to this test harness for easy debugging. In RELEASE, a seperate client application is launched.
        /// </summary>
        public ClientController FixedHarnessClient {  get { return _fixedClient1; } }

        private void Test_StatusChanged(object sender, HarnessTestStatusChangedEventArgs e)
        {
            ITestOnHarness test = (ITestOnHarness)sender;

            if (e.Status == TestStatus.Failed) CurrentTest = null;
            else if (e.Status == TestStatus.InProgress) CurrentTest = test;
            else if (e.Status == TestStatus.NotStarted) CurrentTest = null;
            else if (e.Status == TestStatus.Stopped) CurrentTest = null;
            else if (e.Status == TestStatus.Stopping) CurrentTest = test;
            else CurrentTest = null;
        }

        internal Executing ExecuteMode
        {
            get { lock (classLock) { return _executeMode; } }
            set { lock (classLock) { _executeMode = value; } }
        }



    }



}



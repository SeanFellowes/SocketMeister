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

    internal partial class HarnessClient : IDisposable
    {
        //  Silverlight ports are between 4502-4534
        public const int SilverlightPolicyPort = 943;

        private readonly object classLock = new object();
        private HarnessClientCollection _testClientCollection = new HarnessClientCollection();
        private ITestOnHarness _currentTest = null;
        private Executing _executeMode = Executing.Stopped;
        private static readonly object _lock = new object();
        private readonly HarnessClient _fixedTestClient;
        private readonly PolicyServer policyServer;
        private readonly TestCollection<ITestOnHarness> _tests;

        /// <summary>
        /// Raised when an trace log event has been raised.
        /// </summary>
        internal event EventHandler<TraceEventArgs> TraceEventRaised;

        /// <summary>
        /// The status of the socket server changed. Statuses include stating, started, stopping and stopped.
        /// </summary>
        public event EventHandler<PolicyServer.ServerStatusEventArgs> PolicyServerStatusChanged;




        public HarnessClient()
        {
            _tests = new TestCollection<ITestOnHarness>();

            //  SETUP POLICY SERVER
            policyServer = new PolicyServer();
            policyServer.SocketServerStatusChanged += PolicyServer_SocketServiceStatusChanged;
            policyServer.TraceEventRaised += PolicyServer_TraceEventRaised;


            //  SETUP FIXED CLIENT
            _fixedTestClient = new HarnessClient(int.MaxValue);
#if !DEBUG
            _fixedTestClient.LaunchClientApplication();
#endif

            new Thread(delegate ()
            {
                DateTime maxWait = DateTime.Now.AddSeconds(30);
                while (true == true)
                {
                    if (DateTime.Now > maxWait)
                        throw new TimeoutException("Visual Studio debug mode timed out waiting for the fixed client to connect. Make sure both the harness and client applications are set as Startup Projects.");
                    else if (_fixedTestClient.SocketClient != null)
                        break;
                    else
                        Thread.Sleep(1000);
                }
            }).Start();
        }

        private void PolicyServer_TraceEventRaised(object sender, TraceEventArgs e)
        {
            TraceEventRaised?.Invoke(this, e);
        }

        private void PolicyServer_SocketServiceStatusChanged(object sender, PolicyServer.ServerStatusEventArgs e)
        {
            PolicyServerStatusChanged?.Invoke(sender, e);
        }


        public void Start()
        {
                policyServer.Start();

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



        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (policyServer != null)
                {
                    if (policyServer.Status == SocketServerStatus.Started)
                    {
                        policyServer.Stop();
                    }
                    policyServer.SocketServerStatusChanged -= PolicyServer_SocketServiceStatusChanged;
                    policyServer.TraceEventRaised -= PolicyServer_TraceEventRaised;
                    policyServer.Dispose();
                }
            }
        }


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
        public PolicyServer PolicyServer {  get { return policyServer; } }

        /// <summary>
        /// Suite of tests which are available;
        /// </summary>
        public TestCollection<ITestOnHarness> Tests { get { return _tests; } }


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
        /// A test client which stays open for all testing. This is also where debugging takes place.
        /// </summary>
        public HarnessClient FixedTestClient {  get { return _fixedTestClient; } }

        private void Test_StatusChanged(object sender, TestStatusChangedEventArgs e)
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



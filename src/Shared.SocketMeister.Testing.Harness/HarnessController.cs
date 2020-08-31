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
    internal enum ExecuteModes
    {
        Stopped = 0,
        SingleTest = 1,
        AllTests = 2
    }

    internal class HarnessController : IDisposable
    {
        private bool _disposed = false;
        private bool _disposeCalled = false;
        private int _currentTestPtr = 0;
        private ITestOnHarness _currentTest = null;
        private ExecuteModes _executeMode = ExecuteModes.Stopped;
        private static readonly object _lock = new object();
        private readonly ControlBus.HarnessClientController _fixedClient1;
        private readonly ControlBus.HarnessServerController _fixedServer1;
        private readonly PolicyServer _policyServer;
        private readonly List<ITestOnHarness> _tests = new List<ITestOnHarness>();

        /// <summary>
        /// The execute mode for the test harness changed. SingleTest / AllTests / Stopped
        /// </summary>
        public event EventHandler<EventArgs> ExecuteModeChanged;

        /// <summary>
        /// Raised when an trace log event has been raised.
        /// </summary>
        internal event EventHandler<TraceEventArgs> TraceEventRaised;

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
            ControlBusServer = new ControlBus.ControlBusServer();
            ControlBusServer.RequestReceived += _controlBusServer_RequestReceived;

            //  SETUP POLICY SERVER
            _policyServer = new PolicyServer();
            _policyServer.TraceEventRaised += PolicyServer_TraceEventRaised;

            //  SETUP FIXED SERVER
            _fixedServer1 = new ControlBus.HarnessServerController(Constants.HarnessFixedServerPort, int.MaxValue - 1, Constants.ControlBusServerIPAddress);

            //  SETUP FIXED CLIENT
            _fixedClient1 = new ControlBus.HarnessClientController(int.MaxValue);


            //new Thread(delegate ()
            //{
            //    DateTime maxWait = DateTime.Now.AddSeconds(30);
            //    while (_disposeCalled == false)
            //    {
            //        if (DateTime.Now > maxWait)
            //            throw new TimeoutException("Visual Studio debug mode timed out waiting for the fixed client to connect. Make sure both the harness and client applications are set as Startup Projects.");
            //        else if (_fixedClient1.ListenerClient != null)
            //            break;
            //        else
            //            Thread.Sleep(1000);
            //    }
            //}).Start();
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

                if (ControlBusServer != null)
                {
                    try
                    {
                        ControlBusServer.RequestReceived -= _controlBusServer_RequestReceived;
                        ControlBusServer.Stop();
                        ControlBusServer.Dispose();
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

                _disposed = true;
            }
        }




        private void _controlBusServer_RequestReceived(object sender, SocketServer.RequestReceivedEventArgs e)
        {
            int r = Convert.ToInt32(e.Parameters[0]);
            if (r == ControlBus.ControlMessage.HarnessControlBusClientIsConnecting)
            {
                int ClientId = Convert.ToInt32(e.Parameters[1]);
                if (ClientId == _fixedClient1.ClientId)
                {
                    //  FIXED CLIENT HAS PHONED HOME
                    _fixedClient1.ControlBusListenerClient = e.Client;
                }
                else if (ClientId == _fixedServer1.ClientId)
                {
                    //  FIXED SERVER1 HAS PHONED HOME
                    _fixedServer1.ControlBusListenerClient = e.Client;
                }
                else
                {
                    ////  ANOTHER CLIENT HAS PHONED HOME. FIND THE CLIENT
                    //ControlBus.HarnessClientController client = _testClientCollection[ClientId];
                    //if (client != null)
                    //{
                    //    //  ASSIGN THE SocketMeister Server Client to the class. When connecting a test harness client, this value is checked for NOT null (Connected).
                    //    client.ListenerClient = e.Client;
                    //}
                }
            }
        }

        public ControlBus.ControlBusServer ControlBusServer { get; }

        private int CurrentTestPtr {  get {  lock (_lock) { return _currentTestPtr; } } }

        /// <summary>
        /// In DEBUG, this is attached to this test harness for easy debugging. In RELEASE, a seperate client application is launched.
        /// </summary>
        public ControlBus.HarnessClientController FixedHarnessClient {  get { return _fixedClient1; } }

        public ControlBus.ServerController FixedServer1 {  get { return _fixedServer1; } }


        private void AddTest(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            ITestOnHarness Test = (ITestOnHarness)Activator.CreateInstance(t);
            _tests.Add(Test);
            Test.IsExecutingChanged += Test_IsExecutingChanged;
           
        }

        private void Test_IsExecutingChanged(object sender, EventArgs e)
        {
            CurrentTest = (ITestOnHarness)sender;

            if (ExecuteMode == ExecuteModes.Stopped && CurrentTest.IsExecuting == true)
                ExecuteMode = ExecuteModes.SingleTest;
            else if (ExecuteMode == ExecuteModes.SingleTest && CurrentTest.IsExecuting == false)
                ExecuteMode = ExecuteModes.Stopped;
            else if (ExecuteMode == ExecuteModes.AllTests && CurrentTest.IsExecuting == false)
            {
                lock(_lock)
                {
                    _currentTestPtr++;
                }

                if (CurrentTestPtr < Tests.Count)
                {
                    //  NEXT TEST
                    CurrentTest = Tests[CurrentTestPtr];
                    CurrentTest.Execute();
                }
                else
                {
                    //  FINISHED
                    CurrentTest = null;
                    ExecuteMode = ExecuteModes.Stopped;
                }
            }
        }


        private void PolicyServer_TraceEventRaised(object sender, TraceEventArgs e)
        {
            TraceEventRaised?.Invoke(this, e);
        }

        public void ExecuteAllTests()
        {
            if (ExecuteMode != ExecuteModes.Stopped) throw new ApplicationException("A test is already being executed.");
            foreach (ITestOnHarness test in _tests)
            {
                test.Reset();
            }
            lock (_lock)
            {
                _currentTestPtr = 0;
                _currentTest = _tests[_currentTestPtr];
            }
            ExecuteMode = ExecuteModes.AllTests;
            CurrentTest.Execute();
        }


        public void StopAllTests()
        {
            if (ExecuteMode != ExecuteModes.AllTests) throw new ApplicationException("Execute All Tests is not running.");
            CurrentTest.Stop();
            ExecuteMode = ExecuteModes.Stopped;
        }


        public void Start()
        {
            ControlBusServer.Start();
            _policyServer.Start();
        }


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
            get { lock (_lock) { return _currentTest; } }
            private set 
            { 
                lock (_lock) 
                {
                    if (_currentTest == value) return;
                    _currentTest = value; 
                } 
            }
        }

        internal ExecuteModes ExecuteMode
        {
            get { lock (_lock) { return _executeMode; } }
            private set 
            { 
                lock (_lock) 
                {
                    if (_executeMode == value) return;
                    _executeMode = value; 
                }
                ExecuteModeChanged?.Invoke(this, new EventArgs());
            }
        }
    }



}



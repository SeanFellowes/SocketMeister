﻿using SocketMeister.Testing.ControlBus;
using SocketMeister.Testing.Tests;
using System;
using System.Collections.Generic;
using System.Globalization;

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
        private bool _disposed;
        private bool _disposeCalled;
        private int _currentTestPtr;
        private ITestOnHarness _currentTest = null;
        private ExecuteModes _executeMode = ExecuteModes.Stopped;
        private static readonly object _lock = new object();
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
            ControlBusServer = new ControlBusServer();
            ControlBusServer.MessageReceived += ControlBusServer_MessageReceived;

            //  SETUP FIXED SERVER
            FixedServer1 = new HarnessServerController(int.MaxValue - 1, Constants.ControlBusServerIPAddress);
            FixedServer1.ExceptionRaised += FixedServer1_ExceptionRaised;

            //  SETUP FIXED CLIENT
            FixedClient1 = new HarnessClientController(int.MaxValue);
            FixedClient1.ExceptionRaised += FixedClient1_ExceptionRaised;


            //new Thread(delegate ()
            //{
            //    DateTime maxWait = DateTime.UtcNow.AddSeconds(30);
            //    while (_disposeCalled == false)
            //    {
            //        if (DateTime.UtcNow > maxWait)
            //            throw new TimeoutException("Visual Studio debug mode timed out waiting for the fixed client to connect. Make sure both the harness and client applications are set as Startup Projects.");
            //        else if (_fixedClient1.ListenerClient != null)
            //            break;
            //        else
            //            Thread.Sleep(1000);
            //    }
            //}).Start();
        }

        private void FixedClient1_ExceptionRaised(object sender, ExceptionEventArgs e)
        {
            TraceEventRaised?.Invoke(sender, new TraceEventArgs(e.Exception, e.EventId, nameof(FixedClient1)));
        }

        private void FixedServer1_ExceptionRaised(object sender, ExceptionEventArgs e)
        {
            TraceEventRaised?.Invoke(sender, new TraceEventArgs(e.Exception, e.EventId, nameof(FixedServer1)));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed == true || _disposeCalled == true) return;
            if (disposing)
            {
                _disposeCalled = true;
                if (FixedClient1 != null)
                {
                    try
                    {
                        FixedClient1.Stop();
                    }
                    catch { }
                }

                if (ControlBusServer != null)
                {
                    try
                    {
                        ControlBusServer.MessageReceived -= ControlBusServer_MessageReceived;
                        ControlBusServer.Stop();
                        ControlBusServer.Dispose();
                    }
                    catch { }
                }

                if (FixedServer1 != null)
                {
                    try
                    {
                        FixedServer1.Dispose();
                    }
                    catch { }
                }

                _disposed = true;
            }
        }




        private void ControlBusServer_MessageReceived(object sender, SocketServer.MessageReceivedEventArgs e)
        {
            short messageType = (short)e.Parameters[0];
            if (messageType == ControlMessage.HarnessControlBusClientIsConnecting)
            {
                int ClientId = Convert.ToInt32(e.Parameters[1]);
                if (ClientId == FixedClient1.ClientId)
                {
                    //  FIXED CLIENT HAS PHONED HOME
                    FixedClient1.ControlBusListenerClient = e.Client;
                }
                else if (ClientId == FixedServer1.ClientId)
                {
                    //  FIXED SERVER1 HAS PHONED HOME
                    FixedServer1.ControlBusListenerClient = e.Client;
                }
                else
                    throw new ArgumentOutOfRangeException(nameof(e) + "." + nameof(e.Parameters) + "[0]", "No process defined for " + nameof(e) + "." + nameof(e.Parameters) + "[0] = " + messageType + ".");
            }
        }

        public ControlBusServer ControlBusServer { get; }

        private int CurrentTestPtr { get { lock (_lock) { return _currentTestPtr; } } }

        /// <summary>
        /// In DEBUG, this is attached to this test harness for easy debugging. In RELEASE, a seperate client application is launched.
        /// </summary>
        public HarnessClientController FixedClient1 { get; }

        public HarnessServerController FixedServer1 { get; }


        private void AddTest(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            ITestOnHarness Test = (ITestOnHarness)Activator.CreateInstance(t, this);
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
                lock (_lock)
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
            FixedServer1.Start();
            FixedClient1.Start();
        }


        /// <summary>
        /// Suite of tests which are available;
        /// </summary>
        public List<ITestOnHarness> Tests => _tests;


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



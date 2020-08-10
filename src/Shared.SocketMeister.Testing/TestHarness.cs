#if TESTHARNESS
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SocketMeister.Testing
{
    internal enum Executing
    {
        Stopped = 0,
        SingleTest = 1,
        AllTests = 2,
        StoppingAllTests = 20
    }

    internal class TestHarness : List<ITest>
    {
        private readonly object classLock = new object();
        private TestHarnessClientCollection _clients = new TestHarnessClientCollection();
        private ITest _currentTest = null;
        private Executing _executeMode = Executing.Stopped;
        private static readonly object _lock = new object();
        private static int _maxClientId = 0;

        public TestHarness()
        {
            //  DYNAMICALLY ADD TESTS. TESTS ARE ANY CLASS NAMED BETWEEN SocketMeister.Test001 and SocketMeister.Test999
            //  THIS ELIMINATES HAVING TO ADD CODE HERE WHEN A NEW TEST IS CREATED.
            //  IGNORE TEMPLATE TEST 000
            for (int ctr = 1; ctr <= 999; ctr++ )
            {
                string className = typeof(TestHarness).Namespace + ".Test" + ctr.ToString("000", CultureInfo.InvariantCulture);
                Type t = Type.GetType(className);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
                if (t != null) AddTest(t, ctr);
            }
        }

        /// <summary>
        /// Test Harness Client Collection used during testing.
        /// </summary>
        public TestHarnessClientCollection Clients {  get { return _clients; } }


        public ITest CurrentTest
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

        private void AddTest(Type t, int ctr)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            ITest test;
            object[] parms = new object[2];
            parms[0] = this;
            parms[1] = ctr;

            test = (ITest)Activator.CreateInstance(t, parms);
            test.StatusChanged += Test_StatusChanged;
            Add(test);
        }

        private void Test_StatusChanged(object sender, TestStatusChangedEventArgs e)
        {
            ITest test = (ITest)sender;
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



        /// <summary>
        /// Returns the next sequential number to be uised as a ClientId for tests
        /// </summary>
        /// <returns></returns>
        public static int GetNextClientId()
        {
            lock (_lock)
            {
                _maxClientId++;
                return _maxClientId;
            }
        }
    }



}


#endif
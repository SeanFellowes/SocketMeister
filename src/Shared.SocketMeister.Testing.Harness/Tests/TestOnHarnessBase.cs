using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;


namespace SocketMeister.Testing.Tests
{
    internal partial class TestOnHarnessBase : TestBase<ITestOnHarness>, ITestOnHarness
    {
        private int _percentComplete = 0;
        private TestStatus _status = TestStatus.NotStarted;

        public event EventHandler<EventArgs> ExecuteTest;
        public event EventHandler<TestPercentCompleteChangedEventArgs> PercentCompleteChanged;
        public event EventHandler<HarnessTestStatusChangedEventArgs> StatusChanged;
        public event EventHandler<TraceEventArgs> TraceEventRaised;

        private static int _maxClientId = 0;
        private static readonly object _lockMaxClientId = new object();

        /// <summary>
        /// Raised when a test creates a client
        /// </summary>
        public event EventHandler<HarnessClientEventArgs> ClientCreated;

        /// <summary>
        /// Raised when an an attempt to establish a socket for control messages between a client and server failed.   
        /// </summary>
        public event EventHandler<HarnessClientEventArgs> ClientConnectFailed;


        public TestOnHarnessBase(int Id, string Description) : base(Id, Description)
        {
        }


        /// <summary>
        /// Adds a client to the list and connects it to the test harness control TCP port (Port Constants.HarnessControlBusPort). Opens an instance of the WinForms client app for each client.
        /// </summary>
        /// <returns>The connected (to the test harness control port) client.</returns>
        public HarnessControlBusClientSocketClient AddClient()
        {
            int nextClientId = 0;
            lock (_lockMaxClientId)
            {
                _maxClientId++;
                nextClientId = _maxClientId;
            }
            HarnessControlBusClientSocketClient newClient = new HarnessControlBusClientSocketClient( ControlBusClientType.ClientController, nextClientId);
            ClientCreated?.Invoke(this, new HarnessClientEventArgs(newClient));
            try
            {
                newClient.LaunchClientApplication();
            }
            catch
            {
                if (ClientConnectFailed != null) ClientConnectFailed(this, new HarnessClientEventArgs(newClient));
                throw;
            }

            return newClient;
        }

        /// <summary>
        /// Adds multiple test harness clients (opens an instance of the WinForms client app for each client)
        /// </summary>
        /// <param name="NumberOfClients">Number of test harness clients to run</param>
        /// <returns>List of TestHarnessClient objects</returns>
        public List<HarnessControlBusClientSocketClient> AddClients(int NumberOfClients)
        {
            List<HarnessControlBusClientSocketClient> rVal = new List<HarnessControlBusClientSocketClient>();
            for (int ctr = 1; ctr <= NumberOfClients; ctr++)
            {
                rVal.Add(AddClient());
            }
            return rVal;
        }





        public int PercentComplete
        {
            get { return _percentComplete; }
            internal set
            {
                lock (Lock)
                {
                    if (_percentComplete == value) return;
                    _percentComplete = value;
                }
                if (Parent == null) throw new NullReferenceException("Base class property '" + nameof(Parent) + "' has not been set");
                PercentCompleteChanged?.Invoke(Parent, new TestPercentCompleteChangedEventArgs(value));
            }
        }

        public TestStatus Status
        {
            get { lock (Lock) { return _status; } }
            internal set
            {
                lock (Lock)
                {
                    if (_status == value) return;
                    _status = value;
                }
                if (Parent == null) throw new NullReferenceException("Base class property '" + nameof(Parent) + "' has not been set");
                StatusChanged?.Invoke(Parent, new HarnessTestStatusChangedEventArgs(_parent, value));
            }
        }

        public void RaiseTraceEventRaised(string message, SeverityType severity, int eventId)
        {
            if (Parent == null) throw new NullReferenceException("Base class property 'Parent'has not been set");
            TraceEventRaised?.Invoke(Parent, new TraceEventArgs(message, severity, eventId));
        }

        public void RaiseTraceEventRaised(Exception ex, int eventId)
        {
            if (Parent == null) throw new NullReferenceException("Base class property 'Parent'has not been set");
            TraceEventRaised?.Invoke(Parent, new TraceEventArgs(ex, eventId));
        }


        public void Reset()
        {
            Status = TestStatus.NotStarted;
            PercentComplete = 0;
        }

        public void Start()
        {
            Status = TestStatus.InProgress;
            RaiseTraceEventRaised("Starting Test", SeverityType.Information, 1);
            if (ExecuteTest != null)
            {
                new Thread(new ThreadStart(delegate
                {
                    ExecuteTest(Parent, new EventArgs());
                })).Start();
            }
        }

        public void Stop()
        {
            if (Status != TestStatus.InProgress) return;
            Status = TestStatus.Stopping;
        }


    }
}

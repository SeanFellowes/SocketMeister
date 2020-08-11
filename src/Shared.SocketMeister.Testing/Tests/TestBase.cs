using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

#if TESTHARNESS
using System.Management.Instrumentation;
#endif

namespace SocketMeister.Testing.Tests
{
    internal partial class TestBase
    {
        private readonly int _id;
        private readonly string _description;
        private readonly object _lock = new object();
        private ITest _parent = null;
        private int _percentComplete = 0;
        private TestStatus _status = TestStatus.NotStarted;

        /// <summary>
        /// Raised when a test creates a client
        /// </summary>
        public event EventHandler<ClientEventArgs> ClientCreated;

        /// <summary>
        /// Raised when an an attempt to establish a socket for control messages between a client and server failed.   
        /// </summary>
        public event EventHandler<ClientEventArgs> ClientConnectFailed;

        public event EventHandler<EventArgs> ExecuteTest;
        public event EventHandler<TestPercentCompleteChangedEventArgs> PercentCompleteChanged;
        public event EventHandler<TestStatusChangedEventArgs> StatusChanged;
        public event EventHandler<TraceEventArgs> TraceEventRaised;


        public string Description { get { return _description; } }

        public int Id { get { return _id; } }

        public object Lock { get { return _lock; } }

        internal ITest Parent
        {
            get { lock (_lock) { return _parent; } }
            set { lock (_lock) { _parent = value; } }
        }

        public int PercentComplete
        {
            get { return _percentComplete; }
            internal set
            {
                lock (_lock)
                {
                    if (_percentComplete == value) return;
                    _percentComplete = value;
                }
                if (Parent == null) throw new NullReferenceException("Base class property 'Parent'has not been set");
                PercentCompleteChanged?.Invoke(Parent, new TestPercentCompleteChangedEventArgs(value));
            }
        }

        public TestStatus Status
        {
            get { lock (_lock) { return _status; } }
            internal set
            {
                lock (_lock)
                {
                    if (_status == value) return;
                    _status = value;
                }
                if (Parent == null) throw new NullReferenceException("Base class property '" + nameof(Parent) + "' has not been set");
                StatusChanged?.Invoke(Parent, new TestStatusChangedEventArgs(value));
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

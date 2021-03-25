using System;
using System.Threading;


namespace SocketMeister.Testing.Tests
{
    internal class TestOnHarnessBase : TestBase<ITestOnHarness>, ITestOnHarness
    {
#pragma warning disable IDE0052 // Remove unread private members
        private readonly HarnessController _harnessController;
#pragma warning restore IDE0052 // Remove unread private members
        private bool _isExecuting = false;
        private int _percentComplete = 0;
        private TestStatus _status = TestStatus.NotStarted;

        public event EventHandler<EventArgs> ExecuteTest;
        public event EventHandler<EventArgs> IsExecutingChanged;
        public event EventHandler<TestPercentCompleteChangedEventArgs> PercentCompleteChanged;
        public event EventHandler<HarnessTestStatusChangedEventArgs> StatusChanged;
        public event EventHandler<TraceEventArgs> TraceEventRaised;


        public TestOnHarnessBase(HarnessController HarnessController, int Id, string Description) : base(Id, Description)
        {
            _harnessController = HarnessController;
        }


        /// <summary>
        /// Whether the test is currently executing
        /// </summary>
        public bool IsExecuting
        {
            get { lock (Lock) { return _isExecuting; } }
            private set
            {
                lock (Lock)
                {
                    if (_isExecuting == value) return;
                    _isExecuting = value;
                }
                IsExecutingChanged?.Invoke(this, new EventArgs());
            }
        }

        public int PercentComplete
        {
            get => _percentComplete;
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
                if (value == TestStatus.InProgress || value == TestStatus.Stopping)
                    IsExecuting = true;
                else
                    IsExecuting = false;
            }
        }

        public void RaiseTraceEventRaised(string message, SeverityType severity, int eventId)
        {
            if (Parent == null) throw new NullReferenceException("Base class property 'Parent'has not been set");
            TraceEventRaised?.Invoke(Parent, new TraceEventArgs(message, severity, eventId, base.Name));
        }

        public void RaiseTraceEventRaised(Exception ex, int eventId)
        {
            if (Parent == null) throw new NullReferenceException("Base class property 'Parent'has not been set");
            TraceEventRaised?.Invoke(Parent, new TraceEventArgs(ex, eventId, base.Name));
        }


        public void Reset()
        {
            Status = TestStatus.NotStarted;
            PercentComplete = 0;
        }

        public void Execute()
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
            else
            {
                Status = TestStatus.Stopped;
                RaiseTraceEventRaised("Error: Test is not subscribing to 'ExecuteTest' events.", SeverityType.Error, 2);
            }
        }

        public void Stop()
        {
            new Thread(new ThreadStart(delegate
            {
                if (Status != TestStatus.InProgress) return;
                Status = TestStatus.Stopping;
            })).Start();
        }


    }
}

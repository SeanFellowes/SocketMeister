using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;


namespace SocketMeister.Testing.Tests
{
    internal partial class TestOnClientBase : TestBase<ITestOnClient>, ITestOnClient
    {
        private int _percentComplete = 0;
        private TestStatus _status = TestStatus.NotStarted;

        public event EventHandler<EventArgs> ExecuteTest;
        public event EventHandler<TestPercentCompleteChangedEventArgs> PercentCompleteChanged;
        public event EventHandler<TraceEventArgs> TraceEventRaised;


        public TestOnClientBase(int Id, string Description) : base(Id, Description)
        {
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

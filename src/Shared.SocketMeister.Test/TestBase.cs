using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Instrumentation;
using System.Text;
using System.Threading;

namespace SocketMeister
{
    internal class TestBase
    {
        private static int _clientId = 0;

        private readonly int _id;
        private readonly string _description;
        private readonly object _lock = new object();
        private ITest _parent = null;
        private int _percentComplete = 0;
        private TestStatus _status = TestStatus.NotStarted;

        public event EventHandler<EventArgs> ExecuteTest;
        public event EventHandler<TestPercentCompleteChangedEventArgs> PercentCompleteChanged;
        public event EventHandler<TestStatusChangedEventArgs> StatusChanged;
        public event EventHandler<TraceEventArgs> TraceEventRaised;

        public TestBase (int Id, string Description)
        {
            _id = Id;
            _description = Description;
        }

        public string Description { get { return _description; } }

        public int Id { get { return _id; } }

        public object Lock {  get {  return _lock; } } 

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
                if (Parent == null) throw new NullReferenceException("Base class property 'Parent'has not been set");
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



        public void CloseClient(string ClientId)
        {

        }

        public int OpenClient()
        {
            int clientId;
            lock(_lock)
            {
                _clientId++;
                clientId = _clientId;
            }

            Process process = new Process();
            process.StartInfo.FileName = @"SocketMeister.Test.Client.WinForms.exe";
            process.StartInfo.Arguments = clientId.ToString();
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = true;
            process.Start();
            DateTime maxWait = DateTime.Now.AddMilliseconds(5000);
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

                Thread.Sleep(250);
            }

            return clientId;
        }





    }
}

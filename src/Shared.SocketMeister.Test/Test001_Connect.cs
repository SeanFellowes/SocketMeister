using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SocketMeister
{
    internal class Test001_Connect : ITest
    {
        private readonly object _classLock = new object();
        private int _percentComplete = 0;
        private TestStatus _status = TestStatus.NotStarted;
        private bool _stop = false;

        public event EventHandler<TestPercentCompleteChangedEventArgs> PercentCompleteChanged;
        public event EventHandler<TestStatusChangedEventArgs> StatusChanged;
        public event EventHandler<TraceEventArgs> TraceEventRaised;

        public int Id { get { return 1; } }
        public string Description {  get { return "Connects blah blah blah"; } }

        public int PercentComplete
        {
            get { return _percentComplete; }
            private set
            {
                lock (_classLock) { if (_percentComplete == value) return; _percentComplete = value; }
                PercentCompleteChanged?.Invoke(this, new TestPercentCompleteChangedEventArgs(value));
            }
        }

        public TestStatus Status 
        { 
            get { return _status; } 
            private set
            {
                lock (_classLock) { _status = value; }
                StatusChanged?.Invoke(this, new TestStatusChangedEventArgs(value));
            }
        }

        public void Reset()
        {
            Status = TestStatus.NotStarted;
            PercentComplete = 0;
        }

        public void Start()
        {
            lock (_classLock) { _stop = false; }
            new Thread(new ThreadStart(delegate { BackgroundStart(); })).Start();
        }

        public void Stop()
        {
            if (Status != TestStatus.InProgress) return;
            Status = TestStatus.Stopping;
            lock (_classLock) { _stop = true; }
        }


        private void BackgroundStart()
        {
            try
            {
                Status = TestStatus.InProgress;
                TraceEventRaised?.Invoke(this, new TraceEventArgs("Starting Test", SeverityType.Information, 1));

                for (int r = 0; r < 20; r++)
                {
                    PercentComplete = Convert.ToInt32(((r + 1.0) / 20) * 100.0);
                    TraceEventRaised?.Invoke(this, new TraceEventArgs("All gooewfon ewoifm oweiAll gooewfon ewoifm oweimf weofim ewofim oimewkfjn fewn iuniu wenf iuwenf iwenf iunwi eufn iun weirf wef we ewf wef wef " + r.ToString(), SeverityType.Information, 1));
                    //Thread.Sleep(50);

                    //if (r == 50)
                    //{
                    //    throw new Exception("A complete failure occured");
                    //}

                    lock (_classLock) 
                    { 
                        if (_stop == true)
                        {
                            Status = TestStatus.Stopped;
                            TraceEventRaised?.Invoke(this, new TraceEventArgs("Test was stopped before completing", SeverityType.Information, 1));
                            return;
                        }
                    }


                }

                Status = TestStatus.Successful;
                TraceEventRaised?.Invoke(this, new TraceEventArgs("Test completed successfully", SeverityType.Information, 1));

            }
            catch (Exception ex)
            {
                Status = TestStatus.Failed;
                TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 1));
            }
        }




    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SocketMeister
{
    internal class Test001_Connect : ITest
    {
        private readonly object classLock = new object();
        private TestStatus status = TestStatus.NotStarted;
        private bool stopped = false;

        public int Id { get { return 1; } }
        public string Description {  get { return "Connects blah blah blah"; } }
        public TestStatus TestStatus { get { return status; } }

        public event EventHandler<TestNotificationEventArgs> TestNotification;
        public event EventHandler<TestStatusChangedEventArgs> TestStatusChanged;
        public event EventHandler<TraceEventArgs> TraceEventRaised;

        public void Start()
        {
            new Thread(new ThreadStart(delegate { BackgroundStart(); })).Start();
        }

        public void Stop()
        {
            new Thread(new ThreadStart(delegate { BackgroundStart(); })).Start();
        }


        private void BackgroundStart()
        {
            try
            {
                TestStatusChanged?.Invoke(this, new TestStatusChangedEventArgs(TestStatus.InProgress));
                TraceEventRaised?.Invoke(this, new TraceEventArgs("Starting Test", SeverityType.Information, 1));

                for (int r = 0; r < 200; r++)
                {
                    TraceEventRaised?.Invoke(this, new TraceEventArgs("All gooewfon ewoifm oweimf weofim ewofim oimewkfjn fewn iuniu wenf iuwenf iwenf iunwi eufn iun weirf wef we ewf wef wef " + r.ToString(), SeverityType.Information, 1));
                    Thread.Sleep(250);

                    //  SEAN SEAN SEAN
                    //if ()
                }
            }
            catch (Exception ex)
            {
                TestStatusChanged?.Invoke(this, new TestStatusChangedEventArgs(TestStatus.Failed));
                TestStatusChanged?.Invoke(this, new TestStatusChangedEventArgs(TestStatus.Failed));
                TraceEventRaised?.Invoke(this, new TraceEventArgs(ex, 1));
            }

        }

    }
}

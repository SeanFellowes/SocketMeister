using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{
    internal class Test001_Connect : ITest
    {
        public TestNotificationStatuses status = TestNotificationStatuses.NoStarted;



        public int Id { get { return 1; } }
        public string Description {  get { return "Connects blah blah blah"; } }
        public TestNotificationStatuses Status { get { return status; } }

        public event EventHandler<TestNotificationEventArgs> TestNotification;

        /// <summary>
        /// Trace message raised from within SocketMeister.
        /// </summary>
        public event EventHandler<TraceEventArgs> TraceEventRaised;



        public void Start()
        {
            TraceEventRaised?.Invoke(this, new TraceEventArgs("Starting Test", SeverityType.Information, 1));
        }

    }
}

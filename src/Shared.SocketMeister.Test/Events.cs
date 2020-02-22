using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{
    internal class TestNotificationEventArgs : EventArgs
    {
        private readonly string message = "";
        private readonly double percentComplete = 0;
        private readonly TestStatus status;

        internal TestNotificationEventArgs(string Message, double PercentComplete, TestStatus Status)
        {
            message = Message;
            percentComplete = PercentComplete;
            status = Status;
        }

 
        public string Message
        {
            get { return message; } 
        }

        public double PercentComplete
        {
            get { return percentComplete; }
        }

        public TestStatus Status
        {
            get { return status; }
        }

    }

    internal class TestStatusChangedEventArgs : EventArgs
    {
        private readonly TestStatus status;

        internal TestStatusChangedEventArgs(TestStatus Status)
        {
            status = Status;
        }

        public TestStatus Status
        {
            get { return status; }
        }

    }

}

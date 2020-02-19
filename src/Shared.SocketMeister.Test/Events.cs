using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{
    internal class TestNotificationEventArgs : EventArgs
    {
        private readonly string message = "";
        private readonly double percentComplete = 0;
        private readonly TestNotificationStatuses status;

        internal TestNotificationEventArgs(string Message, double PercentComplete, TestNotificationStatuses Status)
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

        public TestNotificationStatuses Status
        {
            get { return status; }
        }

    }
}

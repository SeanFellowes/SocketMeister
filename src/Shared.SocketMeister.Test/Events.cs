using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{
    internal class TestPercentCompleteChangedEventArgs : EventArgs
    {
        private readonly int percentComplete;

        internal TestPercentCompleteChangedEventArgs(int PercentComplete)
        {
            if (PercentComplete < 0) percentComplete = 0;
            else if (PercentComplete > 100) percentComplete = 100;
            else percentComplete = PercentComplete;
        }

        public int PercentComplete
        {
            get { return percentComplete; }
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

using System;

namespace SocketMeister.Testing
{

    internal class HarnessClientEventArgs : EventArgs
    {
        private readonly ControlBus.HarnessClientController _client;

        internal HarnessClientEventArgs(ControlBus.HarnessClientController Client)
        {
            _client = Client;
        }

        public ControlBus.HarnessClientController Client
        {
            get { return _client; }
        }

    }


    internal class HarnessTestStatusChangedEventArgs : EventArgs
    {
        private readonly TestStatus _status;
        private readonly ITestOnHarness _test;

        internal HarnessTestStatusChangedEventArgs(ITestOnHarness Test, TestStatus Status)
        {
            _test = Test;
            _status = Status;
        }

        public TestStatus Status
        {
            get { return _status; }
        }

        public ITestOnHarness Test
        {
            get { return _test; }
        }
    }


}

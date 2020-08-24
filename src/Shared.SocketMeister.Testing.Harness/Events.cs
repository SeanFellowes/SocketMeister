using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister.Testing
{

    internal class HarnessClientEventArgs : EventArgs
    {
        private readonly HarnessClient _client;

        internal HarnessClientEventArgs(HarnessClient Client)
        {
            _client = Client;
        }

        public HarnessClient Client
        {
            get { return _client; }
        }

    }


    internal class HarnessTestStatusChangedEventArgs<T> : EventArgs
    {
        private readonly TestStatus _status;
        private readonly T _test;

        internal HarnessTestStatusChangedEventArgs(T Test, TestStatus Status)
        {
            _test = Test;
            _status = Status;
        }

        public TestStatus Status
        {
            get { return _status; }
        }

        public T Test
        {
            get { return _test; }
        }
    }


}

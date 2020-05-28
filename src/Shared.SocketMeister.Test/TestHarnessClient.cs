using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Instrumentation;
using System.Text;
using System.Threading;

namespace SocketMeister
{
    internal class TestHarnessClient
    {
        private readonly int _clientId = 0;
        private readonly object _lock = new object();




        public TestHarnessClient(int ClientId)
        {
            _clientId = ClientId;
        }

    }
}

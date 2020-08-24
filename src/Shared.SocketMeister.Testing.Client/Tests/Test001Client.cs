using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
#if TESTHARNESS
using System.Net.Http.Headers;
#endif

namespace SocketMeister.Testing.Tests
{
    internal class Test001Client : TestOnClientBase, ITestOnClient
    {
        public Test001Client() : base (Test001Base.Id, Test001Base.Description)
        {
            base.Parent = this;   
        }

        


    }
}

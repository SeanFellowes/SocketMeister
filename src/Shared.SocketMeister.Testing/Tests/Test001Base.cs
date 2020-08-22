using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
#if TESTHARNESS
using System.Net.Http.Headers;
#endif

namespace SocketMeister.Testing.Tests
{
    internal partial class Test001Base : TestBase, ITest
    {
        private const string _testDescription = "1 Client, Connect, Valid Operations, Disconnect";
        private const int _testId = 1;

        public Test001Base() : base(_testId, _testDescription) 
        {
        }

        internal new ITest Parent
        {
            get { return base.Parent; }
            set { base.Parent = value;  }
        }



    }
}

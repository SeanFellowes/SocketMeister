using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
#if TESTHARNESS
using System.Net.Http.Headers;
#endif

namespace SocketMeister.Testing.Tests
{
    internal partial class Test001 : TestBase, ITest
    {
        private const string TestDescription = "1 Client, Connect, Valid Operations, Disconnect";

        public Test001(int Id) : base(Id, TestDescription) 
        {
            base.Parent = this;   
        }




    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

#if TESTHARNESS
using System.Management.Instrumentation;
#endif

namespace SocketMeister.Testing.Tests
{
    internal partial class TestBase
    {

        public TestBase(int Id, string Description)
        {
            _id = Id;
            _description = Description;
        }




    }
}

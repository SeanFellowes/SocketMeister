using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{
    internal enum TestStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Stopping = 2,
        Successful = 50,
        Failed = 100,
        Stopped = 200
    }
}

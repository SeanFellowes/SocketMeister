using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister.Testing
{

    /// <summary>
    /// ITest Interface (HOST)
    /// </summary>
    internal partial interface ITest
    {
        event EventHandler<EventArgs> ExecuteTest;
        event EventHandler<TestPercentCompleteChangedEventArgs> PercentCompleteChanged;
        event EventHandler<TestStatusChangedEventArgs> StatusChanged;
        event EventHandler<TraceEventArgs> TraceEventRaised;

        TestHarness TestHarness { get; }
    }
}

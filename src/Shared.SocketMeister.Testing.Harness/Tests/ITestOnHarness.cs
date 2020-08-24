using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister.Testing
{

    /// <summary>
    /// ITest Interface 
    /// </summary>
    internal partial interface ITestOnHarness : ITest<ITestOnHarness>
    {
        void Reset();

        void Start();

        void Stop();

        event EventHandler<EventArgs> ExecuteTest;
        event EventHandler<TestPercentCompleteChangedEventArgs> PercentCompleteChanged;
        event EventHandler<TraceEventArgs> TraceEventRaised;

        int PercentComplete { get; }
        TestStatus Status { get; }

        event EventHandler<HarnessTestStatusChangedEventArgs<ITestOnHarness>> StatusChanged;


    }
}

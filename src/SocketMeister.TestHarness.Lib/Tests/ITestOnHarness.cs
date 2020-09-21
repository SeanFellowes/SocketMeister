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
        void Execute();

        void Reset();

        void Stop();

        event EventHandler<EventArgs> ExecuteTest;
        event EventHandler<EventArgs> IsExecutingChanged;
        event EventHandler<TestPercentCompleteChangedEventArgs> PercentCompleteChanged;
        event EventHandler<TraceEventArgs> TraceEventRaised;

        bool IsExecuting { get; }
        int PercentComplete { get; }
        TestStatus Status { get; }

        event EventHandler<HarnessTestStatusChangedEventArgs> StatusChanged;


    }
}

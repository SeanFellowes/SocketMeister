using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister.Testing
{

    /// <summary>
    /// ITest Interface 
    /// </summary>
    internal partial interface ITest 
    {
        void Reset();

        void Start();

        void Stop();

        string Description { get; }
        int Id { get; }
        object Lock { get; }
        int PercentComplete { get; }
        TestStatus Status { get; }

        event EventHandler<TestStatusChangedEventArgs> StatusChanged;


    }
}

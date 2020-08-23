using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister.Testing
{

    /// <summary>
    /// ITest Interface 
    /// </summary>
    internal interface ITest 
    {
        string Description { get; }
        int Id { get; }
        object Lock { get; }
        int PercentComplete { get; }
        TestStatus Status { get; }

        event EventHandler<TestStatusChangedEventArgs> StatusChanged;


    }
}

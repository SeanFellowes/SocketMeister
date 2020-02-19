using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{


    internal interface ITest 
    {
        event EventHandler<TestNotificationEventArgs> TestNotification;
        event EventHandler<TraceEventArgs> TraceEventRaised;

        void Start();

        string Description { get; }
        int Id { get; }

        TestNotificationStatuses Status { get; }
    }
}

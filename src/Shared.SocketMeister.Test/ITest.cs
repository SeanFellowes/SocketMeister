using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{


    internal interface ITest 
    {
        event EventHandler<TestNotificationEventArgs> TestNotification;
        event EventHandler<TraceEventArgs> TraceEventRaised;
        event EventHandler<TestStatusChangedEventArgs> TestStatusChanged;

        void Start();

        void Stop();


        string Description { get; }
        int Id { get; }

        TestStatus TestStatus { get; }
    }
}

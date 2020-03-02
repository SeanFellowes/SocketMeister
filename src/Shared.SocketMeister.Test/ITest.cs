﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{


    internal interface ITest 
    {
        event EventHandler<TestPercentCompleteChangedEventArgs> PercentCompleteChanged;
        event EventHandler<TestStatusChangedEventArgs> StatusChanged;
        event EventHandler<TraceEventArgs> TraceEventRaised;

        void Reset();

        void Start();

        void Stop();

        string Description { get; }
        int Id { get; }
        int PercentComplete { get; }
        TestStatus Status { get; }
    }
}

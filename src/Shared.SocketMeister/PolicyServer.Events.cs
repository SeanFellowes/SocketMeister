using System;

#if !SILVERLIGHT && !SMNOSERVER

namespace SocketMeister
{
#if SMISPUBLIC
    public partial class PolicyServer : IDisposable
#else
    internal partial class PolicyServer : IDisposable
#endif
    {
        /// <summary>
        /// Raised whenever a policy request is received
        /// </summary>
#pragma warning disable CA1034 // Nested types should not be visible
        public class PolicyRequestReceivedEventArgs : EventArgs
#pragma warning restore CA1034 // Nested types should not be visible
        {
            /// <summary>
            /// The end point (client) requesting the policy file
            /// </summary>
            public string EndPoint { get; set; }
        }



        ///// <summary>
        ///// Status of a socket service.
        ///// </summary>
        //public class ServerStatusEventArgs : EventArgs
        //{
        //    /// <summary>
        //    /// Constructor
        //    /// </summary>
        //    /// <param name="status">Current status of the service</param>
        //    public ServerStatusEventArgs(SocketServerStatus status)
        //    {
        //        Status = status;
        //    }

        //    /// <summary>
        //    /// Execution status of the service.
        //    /// </summary>
        //    public SocketServerStatus Status { get; set; } = SocketServerStatus.Stopped;
        //}


        internal class PolicyServerIsRunningChangedArgs : EventArgs
        {
            public bool IsRunning { set; get; }
        }
    }

}
#endif

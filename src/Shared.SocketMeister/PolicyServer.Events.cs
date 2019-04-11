#if !SILVERLIGHT && !SMNOSERVER
using System;

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
        public class PolicyRequestReceivedEventArgs : EventArgs
        {
            /// <summary>
            /// The end point (client) requesting the policy file
            /// </summary>
            public string EndPoint { get; set; }
        }



        /// <summary>
        /// Status of a socket service.
        /// </summary>
        public class ServerStatusEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="Status">Current status of the service</param>
            public ServerStatusEventArgs(ServiceStatus Status)
            {
                this.Status = Status;
            }

            /// <summary>
            /// Execution status of the service.
            /// </summary>
            public ServiceStatus Status { get; set; } = ServiceStatus.Stopped;
        }


        internal class PolicyServerIsRunningChangedArgs : EventArgs
        {
            public bool IsRunning { set; get; }
        }
    }

}
#endif
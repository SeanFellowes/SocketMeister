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
        /// Error event used in both the Socket Policy Server and Client
        /// </summary>
        public class PolicyServerErrorEventArgs : EventArgs
        {
            /// <summary>
            /// .NET Exception class containing the error
            /// </summary>
            public Exception Error { set; get; }
            /// <summary>
            /// Short description containing the source of the error (eg Server, Client, Listener)
            /// </summary>
            public string Source { set; get; }
        }

        /// <summary>
        /// Raised whenever a policy request is received
        /// </summary>
        public class PolicyRequestReceivedEventArgs : EventArgs
        {
            /// <summary>
            /// The end point (client) requesting the policy file
            /// </summary>
            public string EndPoint { get; set; }
            /// <summary>
            /// The total of policy received since this policy server was instatiated
            /// </summary>
            public int TotalPolicyRequestsReceived { get; set; }
        }



        /// <summary>
        /// Raised whenever the running status of socket server changes
        /// </summary>
        public class PolicyServerStatusChangedEventArgs : EventArgs
        {
            /// <summary>
            /// Status type
            /// </summary>
            public SocketServerStatusTypes Status { get; set; }
            /// <summary>
            /// IP Address of the socket server
            /// </summary>
            public System.Net.IPAddress IPAddress { get; set; }
        }


        internal class PolicyServerIsRunningChangedArgs : EventArgs
        {
            public bool IsRunning { set; get; }
        }

        /// <summary>
        /// Raised whenever an unknown request is received
        /// </summary>
        public class UnknownPolicyRequestReceivedEventArgs : EventArgs
        {
            /// <summary>
            /// The end point (client) submitting the unknown request
            /// </summary>
            public string EndPoint { get; set; }
            /// <summary>
            /// The number of unknown requests received since this policy server was instantiated 
            /// </summary>
            public int TotalUnknownRequestsReceived { get; set; }
        }

    }

}
#endif
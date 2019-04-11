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
        /// Socket server statuses.
        /// </summary>
        public enum SocketServerStatusTypes
        {
            /// <summary>
            /// Socket server is stopped
            /// </summary>
            Stopped = 0,
            /// <summary>
            /// Socket server is started
            /// </summary>
            Started = 1,
            /// <summary>
            /// Socket server is starting
            /// </summary>
            Starting = 10,
            /// <summary>
            /// Socket server is stopping
            /// </summary>
            Stopping = 11
        }
    }
}
#endif
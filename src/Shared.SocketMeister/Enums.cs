using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{

    /// <summary>
    /// Status of a socket service
    /// </summary>
    public enum ServiceStatus
    {
        /// <summary>
        /// Service is stopped
        /// </summary>
        Stopped = 0,
        /// <summary>
        /// Service is starting
        /// </summary>
        Starting = 1,
        /// <summary>
        /// Service is started
        /// </summary>
        Started = 2,
        /// <summary>
        /// Service is stopping
        /// </summary>
        Stopping = 3
    }

}

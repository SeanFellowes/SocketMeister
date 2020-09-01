using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{
    internal static class Constants
    {
        /// <summary>
        /// The number of simultaneous send operations which can take place. Value should be between 2 and 20
        /// </summary>
        public static int SocketAsyncEventArgsPoolSize = 15;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{
    public static class Constants
    {

        /// <summary>
        /// The buffer size to use for sending and receiving data. Note: This value is also used by the 'SocketServer' class.
        /// </summary>
        public static int SEND_RECEIVE_BUFFER_SIZE = 65536;

        /// <summary>
        /// The number of simultaneous send operations which can take place. Value should be between 2 and 20
        /// </summary>
        public static int SocketAsyncEventArgsPoolSize = 15;


    }
}

namespace SocketMeister
{
    internal static class Constants
    {
        /// <summary>
        /// Server and Client exchange this value to determine if they are compatible with each other. 
        /// This value is used to determine version specific operations.
        /// </summary>
        public static int SocketMeisterVersion = 4;

        /// <summary>
        /// The buffer size to use for sending and receiving data. Note: This value is also used by the 'SocketServer' class.
        /// </summary>
        public static int SEND_RECEIVE_BUFFER_SIZE = 65536;

        /// <summary>
        /// Number of seconds to wait before generating a timeout error when sending a response to a request.
        /// </summary>
        public static int SEND_RESPONSE_TIMEOUT_SECONDS = 30;

        /// <summary>
        /// The number of simultaneous send operations which can take place. Value should be between 2 and 20
        /// </summary>
        public static int SocketAsyncEventArgsPoolSize = 20;


    }
}

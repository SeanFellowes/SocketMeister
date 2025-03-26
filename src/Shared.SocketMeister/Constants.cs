namespace SocketMeister
{
    internal static class Constants
    {
        /// <summary>
        /// Server and Client exchange this value to determine if they are compatible with each other. 
        /// This value is used to determine version specific operations.
        /// </summary>
        public static int SOCKET_MEISTER_VERSION = 10;

        /// <summary>
        /// The maximum number of milliseconds to wait for clients to disconnect whien stopping the socket server
        /// </summary>
        public const int MAX_WAIT_FOR_CLIENT_DISCONNECT_WHEN_STOPPING = 30000;

        /// <summary>
        /// The minimum version of the SocketServer that the client can connect to.
        /// </summary>
        public static int MINIMUM_SERVER_VERSION_SUPPORTED_BY_CLIENT = 10;

        /// <summary>
        /// The minimum version of the SocketClient that the server can connect to.
        /// </summary>
        public static int MINIMUM_CLIENT_VERSION_SUPPORTED_BY_SERVER = 10;

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
        public static int SOCKET_ASYNC_EVENT_ARGS_POOL_SIZE = 20;
    }
}

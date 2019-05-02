#if !SILVERLIGHT && !SMNOSERVER
using System;

namespace SocketMeister
{
#if SMISPUBLIC
    public partial class SocketServer
#else
    internal partial class SocketServer
#endif
    {
        /// <summary>
        /// Event raised when a client connects to the socket server.
        /// </summary>
        public class ClientConnectedEventArgs : EventArgs
        {
            internal ClientConnectedEventArgs(Client client)
            {
                Client = client;
            }

            /// <summary>
            /// The client which connected
            /// </summary>
            public Client Client { get; }
        }

        /// <summary>
        /// Event raised when a client disconnects from the socket server.
        /// </summary>
        public class ClientDisconnectedEventArgs : EventArgs
        {
            internal ClientDisconnectedEventArgs(Client client)
            {
                Client = client;
            }

            /// <summary>
            /// The client which disconnected
            /// </summary>
            public Client Client { get; }
        }


        /// <summary>
        /// Values provided when a message is received from a client. 
        /// </summary>
        public class MessageReceivedEventArgs : EventArgs
        {
            internal MessageReceivedEventArgs(Client client, object[] parameters)
            {
                Client = client;
                Parameters = parameters;
            }

            /// <summary>
            /// The client which initiated the message.
            /// </summary>
            public Client Client { get; private set; }

            /// <summary>
            /// The parameters provided with the message.
            /// </summary>
            public object[] Parameters { get; private set; }
        }

        /// <summary>
        /// Values provided when a request is received from a client. 
        /// </summary>
        public class RequestReceivedEventArgs : MessageReceivedEventArgs
        {
            internal RequestReceivedEventArgs(Client client, object[] parameters) : base(client, parameters) { }

            /// <summary>
            /// The byte array which is to be returned to the client. Null is returned if a 'Response' value is not provided when processing the 'RequestReceived' event.
            /// </summary>
            public byte[] Response { get; set; } = null;
        }

        /// <summary>
        /// Status of a socket service.
        /// </summary>
        public class ServerStatusEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="status">Current status of the service</param>
            public ServerStatusEventArgs(ServiceStatus status)
            {
                Status = status;
            }

            /// <summary>
            /// Execution status of the service.
            /// </summary>
            public ServiceStatus Status { get; set; } = ServiceStatus.Stopped;
        }

    }
}
#endif
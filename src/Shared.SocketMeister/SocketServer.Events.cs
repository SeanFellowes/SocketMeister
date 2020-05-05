#if !SILVERLIGHT && !SMNOSERVER
using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{
#if SMISPUBLIC
    public partial class SocketServer
#else
    internal partial class SocketServer
#endif
    {
        /// <summary>
        /// TO BE DEPRICATED. Use ClientsChangedEventArgs. Event raised when a client connects to the socket server.
        /// </summary>
        public class ClientConnectedEventArgs : EventArgs
        {
            internal ClientConnectedEventArgs(Client Client)
            {
                this.Client = Client;
            }

            /// <summary>
            /// The client which connected
            /// </summary>
            public Client Client { get; }
        }

        /// <summary>
        /// TO BE DEPRICATED. Use ClientsChangedEventArgs. Event raised when a client disconnects from the socket server.
        /// </summary>
        public class ClientDisconnectedEventArgs : EventArgs
        {
            internal ClientDisconnectedEventArgs(Client Client)
            {
                this.Client = Client;
            }

            /// <summary>
            /// The client which disconnected
            /// </summary>
            public Client Client { get; }
        }


        /// <summary>
        /// Event raised when there is a change to the clients connected to the socket server.
        /// </summary>
        public class ClientsChangedEventArgs : EventArgs
        {
            internal ClientsChangedEventArgs(int Count)
            {
                this.Count = Count;
            }

            /// <summary>
            /// The numb er of clients connected to the socket server
            /// </summary>
            public int Count { get; }
        }




        /// <summary>
        /// Values provided when a message is received from a client. 
        /// </summary>
        public class MessageReceivedEventArgs : EventArgs
        {
            internal MessageReceivedEventArgs(Client Client, object[] Parameters)
            {
                this.Client = Client;
                this.Parameters = Parameters;
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
            internal RequestReceivedEventArgs(Client Client, object[] Parameters) : base(Client, Parameters) { }

            /// <summary>
            /// The byte array which is to be returned to the client. Null is returned if a 'Response' value is not provided when processing the 'RequestReceived' event.
            /// </summary>
            public byte[] Response { get; set; } = null;
        }

        /// <summary>
        /// Execution status of a service changed. Includes the new status
        /// </summary>
        public class SocketServerStatusChangedEventArgs : EventArgs
        {

            /// <summary>
            /// Default constructor
            /// </summary>
            public SocketServerStatusChangedEventArgs() { }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="Status">Current status of the service</param>
            public SocketServerStatusChangedEventArgs(SocketServerStatus Status)
            {
                this.Status = Status;
            }

            /// <summary>
            /// Execution status of the service.
            /// </summary>
            public SocketServerStatus Status { get; set; } = SocketServerStatus.Stopped;
        }

    }
}
#endif
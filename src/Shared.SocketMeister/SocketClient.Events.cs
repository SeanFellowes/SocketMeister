using System;

namespace SocketMeister
{

    /// <summary>
    /// Asynchronous, persistent TCP/IP socket client supporting multiple destinations
    /// </summary>
#if SMISPUBLIC
    public partial class SocketClient : IDisposable
#else
    internal partial class SocketClient : IDisposable
#endif
    {
        /// <summary>
        /// Information provided when a SocketClient connection to a socket server changes status
        /// </summary>
        public class ConnectionStatusChangedEventArgs : EventArgs
        {
            private readonly object classLock = new object();
            private string iPAddress = "";
            private ushort port = 0;
            private ConnectionStatuses status = ConnectionStatuses.Disconnected;

            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="status">The status of the socket</param>
            internal ConnectionStatusChangedEventArgs(ConnectionStatuses status)
            {
                this.status = status;
            }

            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="status">The status of the socket</param>
            /// <param name="iPAddress">Destination TCP/IP Port.</param>
            /// <param name="port"></param>
            internal ConnectionStatusChangedEventArgs(ConnectionStatuses status, string iPAddress, ushort port)
            {
                this.status = status;
                this.iPAddress = iPAddress;
                this.port = port;
            }

            /// <summary>
            /// If connected, the IP Address which the socket is connected to.
            /// </summary>
            public string IPAddress
            {
                get { lock (classLock) { return iPAddress; } }
                set { lock (classLock) { iPAddress = value; } }
            }

            /// <summary>
            /// The port which the socket is connected to.
            /// </summary>
            public ushort Port
            {
                get { lock (classLock) { return port; } }
                set { lock (classLock) { port = value; } }
            }

            /// <summary>
            /// The connection status to the remote socket server.
            /// </summary>
            public ConnectionStatuses Status
            {
                get { lock (classLock) { return status; } }
                set { lock (classLock) { status = value; } }
            }

            /// <summary>
            /// Description of the connection status.
            /// </summary>
            public string StatusDescription
            {
                get { lock (classLock) { return GetStatusDescription(); } }
            }
            private string GetStatusDescription()
            {
                if (status == ConnectionStatuses.Connected) return "Connected";
                else if (status == ConnectionStatuses.Connecting) return "Connecting";
                else if (status == ConnectionStatuses.Disconnected) return "Disconnected";
                else if (status == ConnectionStatuses.Disconnecting) return "Disconnecting";
                else return "Unknown";
            }


        }




        /// <summary>
        /// Values provided when a message is received from the socket server. 
        /// </summary>
        public class MessageReceivedEventArgs : EventArgs
        {
            private readonly object[] parameters;

            internal MessageReceivedEventArgs(object[] parameters)
            {
                this.parameters = parameters;
            }

            /// <summary>
            /// The parameters provided with the message.
            /// </summary>
            public object[] Parameters { get { return parameters; } }
        }

    }



}

using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister.Testing
{
        /// <summary>
        /// Information provided when a SocketClient connection to a socket server changes status
        /// </summary>
        public class ConnectionStatusChangedEventArgs : EventArgs
        {
            private readonly object classLock = new object();
            private string iPAddress = "";
            private ushort port = 0;
            private ConnectionStatus status = ConnectionStatus.Disconnected;

            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="status">The status of the socket</param>
            internal ConnectionStatusChangedEventArgs(ConnectionStatus status)
            {
                this.status = status;
            }

            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="status">The status of the socket</param>
            /// <param name="iPAddress">Destination TCP/IP Port.</param>
            /// <param name="port"></param>
            internal ConnectionStatusChangedEventArgs(ConnectionStatus status, string iPAddress, ushort port)
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
            public ConnectionStatus Status
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
                if (status == ConnectionStatus.Connected) return "Connected";
                else if (status == ConnectionStatus.Connecting) return "Connecting";
                else if (status == ConnectionStatus.Disconnected) return "Disconnected";
                else if (status == ConnectionStatus.Disconnecting) return "Disconnecting";
                else return "Unknown";
            }
        }


}

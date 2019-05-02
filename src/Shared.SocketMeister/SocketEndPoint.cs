using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace SocketMeister
{
    /// <summary>
    /// Client Socket to a SocketServer end point
    /// </summary>
#if SMISPUBLIC
    public class SocketEndPoint : IDisposable
#else
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", MessageId = "isChecked")]
    internal class SocketEndPoint : IDisposable
#endif
    {
        private readonly object classLock = new object();
        private DateTime dontReconnectUntil = DateTime.Now;
        private readonly IPEndPoint ipEndPoint = null;
        private readonly string iPAddress = null;
        private readonly ushort port = 0;
        private readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="iPAddress">IP Address of the server to connect to</param>
        /// <param name="port">Port number of the socket listener to connect to</param>
        [SuppressMessage("Microsoft.Performance", "IDE0018:VariableDeclarationCanBeInlined", MessageId = "NotSupportedBeforeDotNet4")]
        public SocketEndPoint(string iPAddress, int port)
        {
            //  VALIDATE
            if (string.IsNullOrEmpty(iPAddress) == true) throw new Exception("IP Address cannot be null or empty");
            if (port < 1024 || port > ushort.MaxValue) throw new Exception("Port " + port + " must be between 1024 and " + ushort.MaxValue);

            this.iPAddress = iPAddress;
            this.port = Convert.ToUInt16(port);

            //  TRY TO CREATE IpAddress
            IPAddress IPAddr;
            if (System.Net.IPAddress.TryParse(this.iPAddress, out IPAddr))
            {
                switch (IPAddr.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        break;  //  OKAY
                    case AddressFamily.InterNetworkV6:
                        throw new Exception("IP address (" + iPAddress + ") cannot be of type 'InterNetworkV6'. Only IPv4 (e.g. '192.168.23.56') values are acceptable");
                    default:
                        break;  //  OKAY
                }
            }
            else throw new Exception("Unable to create an IPAddress from the provided IPAddress '" + iPAddress + "' and Port " + port);

            ipEndPoint = new IPEndPoint(IPAddr, this.port);

        }

        /// <summary>
        /// Dispose of the class
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the class
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //  NOTE: If you application uses .NET 2.0 or .NET 3.5. add NET20 or NET35 as a conditional compilation symbol, in your project's Build properties
#if !NET35 && !NET20
                socket.Dispose(); 
#else
                if (socket.Connected == true) socket.Close();
#endif
            }
        }


        /// <summary>
        /// Used to delay reconnecting to a server after a server has disconnected or a socket has failed to a server.
        /// </summary>
        internal DateTime DontReconnectUntil
        {
            get { lock (classLock) { return dontReconnectUntil; } }
            set { lock (classLock) { dontReconnectUntil = value; } }
        }

        /// <summary>
        /// IP Address of the server to connect to
        /// </summary>
        public string IPAddress { get { return iPAddress; } }

        /// <summary>
        /// IPEndpoint
        /// </summary>
        internal IPEndPoint IPEndPoint { get { return ipEndPoint; } }

        /// <summary>
        /// Port number of the socket listener to connect to
        /// </summary>
        public ushort Port { get { return port; } }

        /// <summary>
        /// TCP Socket in use for the current destination.
        /// </summary>
        internal Socket Socket
        {
            get { lock (classLock) { return socket; } }
        }

        /// <summary>
        /// Closes the socket
        /// </summary>
        public void CloseSocket()
        {
            lock (classLock)
            {
                if ( socket.Connected == true)
                {
                    socket.Shutdown(SocketShutdown.Both);
#if SILVERLIGHT
                    socket.Close(); 
#else
                    socket.Disconnect(true);
#endif
                }
            }
        }
    }
}

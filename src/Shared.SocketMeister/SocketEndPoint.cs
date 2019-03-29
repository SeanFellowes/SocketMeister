using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketMeister
{
    /// <summary>
    /// Client Socket to a SocketServer end point
    /// </summary>
#if SMISPUBLIC
    public class SocketEndPoint
#else
    internal class SocketEndPoint
#endif
    {
        private DateTime _dontReconnectUntil = DateTime.Now;
        private IPEndPoint _ipEndPoint = null;
        private readonly string _iPAddress = null;
        private readonly object _lock = new object();
        private readonly ushort _port = 0;
        private readonly Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="IPAddress">IP Address of the server to connect to</param>
        /// <param name="Port">Port number of the socket listener to connect to</param>
        public SocketEndPoint(string IPAddress, int Port)
        {
            //  VALIDATE
            if (string.IsNullOrEmpty(IPAddress) == true) throw new Exception("IP Address cannot be null or empty");
            if (Port < 1024 || Port > ushort.MaxValue) throw new Exception("Port " + Port + " must be between 1024 and " + ushort.MaxValue);

            _iPAddress = IPAddress;
            _port = Convert.ToUInt16(Port);

            //  TRY TO CREATE IpAddress
            IPAddress IPAddr;
            if (System.Net.IPAddress.TryParse(_iPAddress, out IPAddr))
            {
                switch (IPAddr.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        break;  //  OKAY
                    case AddressFamily.InterNetworkV6:
                        throw new Exception("IP address (" + IPAddress + ") cannot be of type 'InterNetworkV6'. Only IPv4 (e.g. '192.168.23.56') values are acceptable");
                    default:
                        break;  //  OKAY
                }
            }
            else throw new Exception("Unable to create an IPAddress from the provided IPAddress '" + IPAddress + "' and Port " + Port);

            _ipEndPoint = new IPEndPoint(IPAddr, _port);

        }

        /// <summary>
        /// Used to delay reconnecting to a server after a server has disconnected or a socket has failed to a server.
        /// </summary>
        internal DateTime DontReconnectUntil
        {
            get { lock (_lock) { return _dontReconnectUntil; } }
            set { lock (_lock) { _dontReconnectUntil = value; } }
        }

        /// <summary>
        /// IP Address of the server to connect to
        /// </summary>
        public string IPAddress { get { return _iPAddress; } }

        /// <summary>
        /// IPEndpoint
        /// </summary>
        internal IPEndPoint IPEndPoint { get { return _ipEndPoint; } }

        /// <summary>
        /// Port number of the socket listener to connect to
        /// </summary>
        public ushort Port { get { return _port; } }

        /// <summary>
        /// TCP Socket in use for the current destination.
        /// </summary>
        internal Socket Socket
        {
            get { lock (_lock) { return _socket; } }
        }

        /// <summary>
        /// Closes the socket
        /// </summary>
        public void CloseSocket()
        {
            lock (_lock)
            {
                try { _socket.Shutdown(SocketShutdown.Both); }
                catch { }
#if SILVERLIGHT
                try { _socket.Close(); }
                catch { }
#else
                try { _socket.Disconnect(true); }
                catch { }
#endif
            }
        }
    }
}

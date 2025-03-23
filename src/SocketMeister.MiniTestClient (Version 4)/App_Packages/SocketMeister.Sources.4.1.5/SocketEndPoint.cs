#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0018 // Inline variable declaration
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable CA1303 // Do not pass literals as localized parameters
#pragma warning disable CA1805 // Do not initialize unnecessarily
#pragma warning disable CA1812 // Avoid uninstantiated internal classes

using System;
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
    internal class SocketEndPoint : IDisposable
#endif
    {
        private DateTime _dontReconnectUntil = DateTime.UtcNow.AddYears(-1);
        private readonly IPEndPoint _ipEndPoint = null;
        private readonly string _iPAddress = null;
        private bool _isDisposed = false;
        private readonly object _lock = new object();
        private readonly object _lockSocket = new object();
        private readonly ushort _port = 0;
        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="IPAddress">IP Address of the server to connect to</param>
        /// <param name="Port">Port number of the socket listener to connect to</param>
        public SocketEndPoint(string IPAddress, int Port)
        {
            //  VALIDATE
            if (string.IsNullOrEmpty(IPAddress) == true) throw new ArgumentException("IP Address cannot be null or empty", nameof(IPAddress));
            if (Port < 1024 || Port > ushort.MaxValue) throw new ArgumentException("Port " + Port + " must be between 1024 and " + ushort.MaxValue, nameof(Port));

            _iPAddress = IPAddress;
            _port = Convert.ToUInt16(Port);

            //  TRY TO CREATE IpAddress
            System.Net.IPAddress IPAddr;
            if (System.Net.IPAddress.TryParse(_iPAddress, out IPAddr))
            {
                switch (IPAddr.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        break;  //  OKAY
                    case AddressFamily.InterNetworkV6:
                        throw new ArgumentException("IP address (" + IPAddress + ") cannot be of type 'InterNetworkV6'. Only IPv4 (e.g. '192.168.23.56') values are acceptable", nameof(IPAddress));
                    default:
                        break;  //  OKAY
                }
            }
            else throw new ArgumentException("IP Address '" + IPAddress + "' is invalid", nameof(IPAddress));

            _ipEndPoint = new IPEndPoint(IPAddr, _port);

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
            if (disposing && IsDisposed == false)
            {
                //  NOTE: You may need to define NET35 as a conditional compilation symbol in your project's Build properties
#if !NET35
                try { _socket.Dispose(); }
                catch { }
#else
                try { _socket.Close(); }
                catch { }
#endif

                IsDisposed = true;
            }
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
        public string IPAddress => _iPAddress;

        /// <summary>
        /// IPEndpoint
        /// </summary>
        internal IPEndPoint IPEndPoint => _ipEndPoint;


        private bool IsDisposed
        {
            get { lock (_lock) { return _isDisposed; } }
            set { lock (_lock) { _isDisposed = value; } }
        }

        /// <summary>
        /// Port number of the socket listener to connect to
        /// </summary>
        public ushort Port => _port;

        /// <summary>
        /// TCP Socket in use for the current destination.
        /// </summary>
        internal Socket Socket
        {
            get { lock (_lockSocket) { return _socket; } }
        }

        /// <summary>
        /// Creates a new socket.
        /// </summary>
        internal void RecreateSocket()
        {
            lock (_lockSocket)
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
        }

    }
}

#pragma warning restore CA1812 // Avoid uninstantiated internal classes
#pragma warning restore CA1805 // Do not initialize unnecessarily
#pragma warning restore CA1031 // Do not catch general exception types
#pragma warning restore CA1303 // Do not pass literals as localized parameters
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0018 // Inline variable declaration
#pragma warning restore IDE0079 // Remove unnecessary suppression


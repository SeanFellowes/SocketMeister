using System;
using System.Net;
using System.Net.Sockets;

namespace SocketMeister
{
    /// <summary>
    /// Client <see cref="Socket"/> to a SocketServer end point.
    /// </summary>
#if SMISPUBLIC
    public class SocketEndPoint : IDisposable
#else
    internal class SocketEndPoint : IDisposable
#endif
    {
        private DateTime _dontReconnectUntil;
        private readonly IPEndPoint _ipEndPoint = null;
        private readonly string _iPAddress = null;
        private bool _isDisposed = false;
        private ClientDisconnectReason _lastDisconnectReason = ClientDisconnectReason.Unknown;
        private readonly object _lock = new object();
        private readonly ushort _port = 0;
        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private readonly object _socketLock = new object();

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="IPAddress">IP Address of the server to connect to.</param>
        /// <param name="Port">Port number of the socket listener to connect to.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="IPAddress"/> is null, empty, or invalid, or when <paramref name="Port"/> is out of range.</exception>
        public SocketEndPoint(string IPAddress, int Port)
        {
            //  VALIDATE
            if (string.IsNullOrEmpty(IPAddress) == true) throw new ArgumentException("IP Address cannot be null or empty", nameof(IPAddress));
            if (Port < 1024 || Port > ushort.MaxValue) throw new ArgumentException("Port " + Port + " must be between 1024 and " + ushort.MaxValue, nameof(Port));

            _iPAddress = IPAddress;
            _port = Convert.ToUInt16(Port);

            //  In situations where there are multiple endpoints we wan't to randomize the reconnect time so that all endpoints don't try to reconnect at the same time.
            Random random = new Random(Guid.NewGuid().GetHashCode());
            _dontReconnectUntil = DateTime.Now.AddHours(-(random.Next(10, 2000)));


            //  TRY TO CREATE IpAddress
            IPAddress IPAddr;
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
        /// Dispose of the class.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from <see cref="Dispose()"/> or a finalizer.</param>
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
        /// Description of the IP Address and Port. For example, 123.123.123.123:5000
        /// </summary>
        public string Description
        {
            get { return _iPAddress + ":" + _port; }
        }


        /// <summary>
        /// Used to delay reconnecting to a server after a server has disconnected or a <see cref="Socket"/> has failed to a server.
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
        /// The last reason the client disconnected. Used to calculate reconnect delay.
        /// </summary>
        /// <seealso cref="ClientDisconnectReason"/>
        internal ClientDisconnectReason LastDisconnectReason
        {
            get { lock (_lock) { return _lastDisconnectReason; } }
        }


        /// <summary>
        /// Port number of the socket listener to connect to
        /// </summary>
        public ushort Port => _port;

        /// <summary>
        /// TCP <see cref="Socket"/> in use for the current destination.
        /// </summary>
        internal Socket Socket
        {
            get { lock (_socketLock) { return _socket; } }
        }

        /// <summary>
        /// Sets the client as disconnected and updates the reconnect delay based on the specified <see cref="ClientDisconnectReason"/>.
        /// </summary>
        /// <param name="reason">The reason for the disconnection.</param>
        internal void SetDisconnected(ClientDisconnectReason reason)
        {
            lock (_lock)
            {
                _lastDisconnectReason = reason;

                switch (_lastDisconnectReason)
                {
                    case ClientDisconnectReason.HandshakeTimeout:
                        _dontReconnectUntil = DateTime.UtcNow.AddSeconds(15);
                        break;
                    case ClientDisconnectReason.PollingTimeout:
                        _dontReconnectUntil = DateTime.UtcNow.AddSeconds(15);
                        break;
                    case ClientDisconnectReason.IncompatibleServerVersion:
                        _dontReconnectUntil = DateTime.UtcNow.AddSeconds(60);
                        break;
                    case ClientDisconnectReason.IncompatibleClientVersion:
                        _dontReconnectUntil = DateTime.UtcNow.AddSeconds(60);
                        break;
                    case ClientDisconnectReason.ConnectionReset:
                        _dontReconnectUntil = DateTime.UtcNow.AddSeconds(10);
                        break;
                    case ClientDisconnectReason.SocketError:
                        _dontReconnectUntil = DateTime.UtcNow.AddSeconds(30);
                        break;
                    case ClientDisconnectReason.SocketConnectionRefused:
                        _dontReconnectUntil = DateTime.UtcNow.AddSeconds(7);
                        break;
                    case ClientDisconnectReason.SocketConnectionTimeout:
                        _dontReconnectUntil = DateTime.UtcNow.AddSeconds(7);
                        break;
                    case ClientDisconnectReason.ClientIsStopping:
                        _dontReconnectUntil = DateTime.MaxValue;    // Do not reconnect
                        break;
                    case ClientDisconnectReason.ServerIsStopping:
                        _dontReconnectUntil = DateTime.UtcNow.AddSeconds(15);
                        break;
                    default:
                        _dontReconnectUntil = DateTime.UtcNow.AddSeconds(10);
                        break;
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="Socket"/>.
        /// </summary>
        internal void RecreateSocket()
        {
            lock (_socketLock)
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
        }

    }
}

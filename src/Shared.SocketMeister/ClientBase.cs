using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;

namespace SocketMeister
{
    /// <summary>
    /// Base class for SocketClient and SocketServer.Client
    /// </summary>
#if SMISPUBLIC
    public abstract class ClientBase : IDisposable
#else
    internal abstract class ClientBase : IDisposable
#endif
    {
        private bool _disposed = false;
        private readonly object _lock = new object();
        private string _ipAddress;
        private string _friendlyName;
        private Guid _clientId;
        private string _clientSocketMeisterVersion;
        private string _serverSocketMeisterVersion;
        private readonly UnrespondedMessageCollection _unrespondedMessages = new UnrespondedMessageCollection();

        /// <summary>
        /// Constructor
        /// </summary>
        public ClientBase(bool isServerImplimentation)
        {
            if (isServerImplimentation)
            {
                _clientId = Guid.NewGuid();

                // Populate SocketMeisterVersion with the version of the current assembly
                try
                {
                    _serverSocketMeisterVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
                }
                catch
                {
                    _serverSocketMeisterVersion = "Unknown";
                }

            }
            else
            {
                // Populate IPAddress with the local machine's IP address
                try
                {
                    _ipAddress = Dns.GetHostAddresses(Dns.GetHostName())
                                   .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString();
                }
                catch
                {
                    _ipAddress = "Unknown";
                }

                // Populate SocketMeisterVersion with the version of the current assembly
                try
                {
                    _clientSocketMeisterVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
                }
                catch
                {
                    _clientSocketMeisterVersion = "Unknown";
                }
            }
        }

        /// <summary>
        /// Disposes of the resources used by the class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources.
        /// </summary>
        /// <param name="disposing">True if managed resources should be released.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here if any
                }
                _unrespondedMessages.Clear(); // Explicitly clear any remaining references

                // Release unmanaged resources if any
                _disposed = true;
            }
        }

        /// <summary>
        /// Disposes of the resources used by the class.
        /// </summary>
        ~ClientBase()
        {
            Dispose(false);
        }


        /// <summary>
        /// The IP address of the client.
        /// </summary>
        public string IPAddress
        {
            get { lock (_lock) { return _ipAddress; } }
            set { lock (_lock) { _ipAddress = value; } }
        }

        /// <summary>
        /// A friendly name for the client. If available, the SocketServer will use this in logging and error handling
        /// </summary>
        public string FriendlyName
        {
            get { lock (_lock) { return _friendlyName; } }
            set
            {
                lock (_lock)
                {
                    if (!string.IsNullOrEmpty(value) && value.Length > 1000)
                        throw new ArgumentException("FriendlyName can be no longer than 1000 characters.", nameof(FriendlyName));
                    _friendlyName = value;
                }
            }
        }

        /// <summary>
        /// The unique identifier for the client.
        /// </summary>
        public Guid ClientId
        {
            get { lock (_lock) { return _clientId; } }
            set { lock (_lock) { _clientId = value; } }
        }

        /// <summary>
        /// The version of SocketMeister used by the client.
        /// </summary>
        public string ClientSocketMeisterVersion
        {
            get { lock (_lock) { return _clientSocketMeisterVersion; } }
            set { lock (_lock) { _clientSocketMeisterVersion = value; } }
        }

        /// <summary>
        /// The version of SocketMeister used by the server.
        /// </summary>
        public string ServerSocketMeisterVersion
        {
            get { lock (_lock) { return _serverSocketMeisterVersion; } }
            set { lock (_lock) { _serverSocketMeisterVersion = value; } }
        }


        internal UnrespondedMessageCollection UnrespondedMessages
        {
            get { return _unrespondedMessages; }
        }


        /// <summary>
        /// Serializes the object to a byte array.
        /// </summary>
        public byte[] Serialize()
        {
            lock (_lock)
            {
                using (var memoryStream = new MemoryStream())
                using (var writer = new BinaryWriter(memoryStream))
                {
                    writer.Write(!string.IsNullOrEmpty(_ipAddress));
                    if (!string.IsNullOrEmpty(_ipAddress))
                        writer.Write(_ipAddress);

                    writer.Write(!string.IsNullOrEmpty(_friendlyName));
                    if (!string.IsNullOrEmpty(_friendlyName))
                        writer.Write(_friendlyName);

                    writer.Write(_clientId != Guid.Empty);
                    if (_clientId != Guid.Empty)
                        writer.Write(_clientId.ToString("D"));

                    writer.Write(!string.IsNullOrEmpty(_clientSocketMeisterVersion));
                    if (!string.IsNullOrEmpty(_clientSocketMeisterVersion))
                        writer.Write(_clientSocketMeisterVersion);

                    return memoryStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Deserializes the object from a byte array.
        /// </summary>
        public void Deserialize(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Invalid data for deserialization.", nameof(data));

            lock (_lock)
            {
                using (var memoryStream = new MemoryStream(data))
                using (var reader = new BinaryReader(memoryStream))
                {
                    if (reader.ReadBoolean())
                        _ipAddress = reader.ReadString();
                    else
                        _ipAddress = null;

                    if (reader.ReadBoolean())
                        _friendlyName = reader.ReadString();
                    else
                        _friendlyName = null;

                    if (reader.ReadBoolean())
                    {
                        string guidString = reader.ReadString();
                        _clientId = new Guid(guidString);
                    }
                    else
                        _clientId = Guid.Empty;

                    if (reader.ReadBoolean())
                        _clientSocketMeisterVersion = reader.ReadString();
                    else
                        _clientSocketMeisterVersion = null;
                }
            }
        }
    }
}

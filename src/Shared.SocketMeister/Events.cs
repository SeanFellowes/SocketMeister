using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{

    /// <summary>
    /// Information provided when a SocketClient connection to a socket server changes status
    /// </summary>
    public class ConnectionStatusChangedEventArgs : EventArgs
    {
        private string _iPAddress = "";
        private readonly object _lock = new object();
        private ushort _port = 0;
        private ConnectionStatus _status = ConnectionStatus.Disconnected;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="Status">The status of the socket</param>
        internal ConnectionStatusChangedEventArgs(ConnectionStatus Status)
        {
            _status = Status;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="Status">The status of the socket</param>
        /// <param name="IPAddress">Destination TCP/IP Port.</param>
        /// <param name="Port"></param>
        internal ConnectionStatusChangedEventArgs(ConnectionStatus Status, string IPAddress, ushort Port)
        {
            _status = Status;
            _iPAddress = IPAddress;
            _port = Port;
        }




        /// <summary>
        /// If connected, the IP Address which the socket is connected to.
        /// </summary>
        public string IPAddress
        {
            get { lock (_lock) { return _iPAddress; } }
            set { lock (_lock) { _iPAddress = value; } }
        }

        /// <summary>
        /// The port which the socket is connected to.
        /// </summary>
        public ushort Port
        {
            get { lock (_lock) { return _port; } }
            set { lock (_lock) { _port = value; } }
        }

        /// <summary>
        /// The connection status to the remote socket server.
        /// </summary>
        public ConnectionStatus Status
        {
            get { lock (_lock) { return _status; } }
            set { lock (_lock) { _status = value; } }
        }

        /// <summary>
        /// Description of the connection status.
        /// </summary>
        public string StatusDescription
        {
            get { lock (_lock) { return GetStatusDescription(); } }
        }

        private string GetStatusDescription()
        {
            if (_status == ConnectionStatus.Connected) return "Connected";
            else if (_status == ConnectionStatus.Connecting) return "Connecting";
            else if (_status == ConnectionStatus.Disconnected) return "Disconnected";
            else if (_status == ConnectionStatus.Disconnecting) return "Disconnecting";
            else return "Unknown";
        }


    }




    /// <summary>
    /// Values provided when a message is received from the socket server. 
    /// </summary>
    internal class MessageReceivedEventArgs : EventArgs
    {
        private readonly object[] _parameters;

        internal MessageReceivedEventArgs(object[] Parameters)
        {
            _parameters = Parameters;
        }

        /// <summary>
        /// The parameters provided with the message.
        /// </summary>
        public object[] Parameters { get { return _parameters; } }
    }




}

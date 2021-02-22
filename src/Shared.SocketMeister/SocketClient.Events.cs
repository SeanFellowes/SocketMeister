using System;
using System.Collections.Generic;
using System.Text;

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
            private string _iPAddress = "";
            private readonly object _lock = new object();
            private ushort _port;
            private ConnectionStatuses _status = ConnectionStatuses.Disconnected;

            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="Status">The status of the socket</param>
            internal ConnectionStatusChangedEventArgs(ConnectionStatuses Status)
            {
                _status = Status;
            }

            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="Status">The status of the socket</param>
            /// <param name="IPAddress">Destination TCP/IP Port.</param>
            /// <param name="Port"></param>
            internal ConnectionStatusChangedEventArgs(ConnectionStatuses Status, string IPAddress, ushort Port)
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
            public ConnectionStatuses Status
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
                if (_status == ConnectionStatuses.Connected) return "Connected";
                else if (_status == ConnectionStatuses.Connecting) return "Connecting";
                else if (_status == ConnectionStatuses.Disconnected) return "Disconnected";
                else if (_status == ConnectionStatuses.Disconnecting) return "Disconnecting";
                else return "Unknown";
            }


        }



        /// <summary>
        /// Values provided when a message is received from the socket server. 
        /// </summary>
        public class MessageReceivedEventArgs : EventArgs
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


        /// <summary>
        /// Values provided when a request is received from a server. 
        /// </summary>
        public class RequestReceivedEventArgs : MessageReceivedEventArgs
        {
            internal RequestReceivedEventArgs(object[] Parameters) : base(Parameters) { }

            /// <summary>
            /// The byte array which is to be returned to the client. Null is returned if a 'Response' value is not provided when processing the 'RequestReceived' event.
            /// </summary>
            public byte[] Response { get; set; } = null;
        }



        /// <summary>
        /// Values provided when a message is received from the socket server. 
        /// </summary>
        public class SubscriptionMessageReceivedEventArgs : EventArgs
        {
            private readonly string _subscriptionName;
            private readonly object[] _parameters;

            internal SubscriptionMessageReceivedEventArgs(string SubscriptionName, object[] Parameters)
            {
                _subscriptionName = SubscriptionName;
                _parameters = Parameters;
            }

            /// <summary>
            /// The parameters provided with the message.
            /// </summary>
            public object[] Parameters { get { return _parameters; } }

            /// <summary>
            /// The name of the subscription 
            /// </summary>
            public string SubscriptionName {  get { return _subscriptionName; } }
        }


    }



}

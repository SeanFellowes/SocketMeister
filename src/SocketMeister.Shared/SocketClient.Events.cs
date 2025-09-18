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
        /// Provides values when a connection attempt to an endpoint fails before the client is fully connected (handshake complete).
        /// Useful for observing transient failures such as connection refused or timeouts while the client remains in the Connecting state.
        /// </summary>
#if SMISPUBLIC
        public class ConnectionAttemptFailedEventArgs : EventArgs
#else
        internal class ConnectionAttemptFailedEventArgs : EventArgs
#endif
        {
            private readonly SocketEndPoint _endPoint;
            private readonly ClientDisconnectReason _reason;
            private readonly string _message;

            internal ConnectionAttemptFailedEventArgs(SocketEndPoint endPoint, ClientDisconnectReason reason, string message)
            {
                _endPoint = endPoint;
                _reason = reason;
                _message = message;
            }

            /// <summary>
            /// The endpoint for which the connection attempt failed.
            /// </summary>
            public SocketEndPoint EndPoint => _endPoint;

            /// <summary>
            /// The reason associated with the failed attempt.
            /// </summary>
            public ClientDisconnectReason Reason => _reason;

            /// <summary>
            /// Optional descriptive message providing context about the failure.
            /// </summary>
            public string Message => _message;
        }
        /// <summary>
        /// Values provided when the connection status changes.
        /// </summary>
#if SMISPUBLIC
        public class ConnectionStatusChangedEventArgs : EventArgs
#else
        internal class ConnectionStatusChangedEventArgs : EventArgs
#endif
        {
            private readonly SocketClient.ConnectionStatuses _oldStatus;
            private readonly SocketClient.ConnectionStatuses _newStatus;
            private readonly SocketEndPoint _endPoint;
            private readonly ClientDisconnectReason _reason;

            internal ConnectionStatusChangedEventArgs(SocketClient.ConnectionStatuses oldStatus, SocketClient.ConnectionStatuses newStatus, SocketEndPoint endPoint, ClientDisconnectReason reason)
            {
                _oldStatus = oldStatus;
                _newStatus = newStatus;
                _endPoint = endPoint;
                _reason = reason;
            }

            /// <summary>
            /// The previous connection status as seen by callers.
            /// </summary>
            public SocketClient.ConnectionStatuses OldStatus => _oldStatus;

            /// <summary>
            /// The new connection status as seen by callers.
            /// </summary>
            public SocketClient.ConnectionStatuses NewStatus => _newStatus;

            /// <summary>
            /// The endpoint associated with the connection state.
            /// </summary>
            public SocketEndPoint EndPoint => _endPoint;

            /// <summary>
            /// The disconnect reason when transitioning to Disconnected; Unknown otherwise.
            /// </summary>
            public ClientDisconnectReason Reason => _reason;
        }

        /// <summary>
        /// Values provided when the current endpoint changes.
        /// </summary>
#if SMISPUBLIC
        public class CurrentEndPointChangedEventArgs : EventArgs
#else
        internal class CurrentEndPointChangedEventArgs : EventArgs
#endif
        {
            private readonly SocketEndPoint _oldEndPoint;
            private readonly SocketEndPoint _newEndPoint;

            internal CurrentEndPointChangedEventArgs(SocketEndPoint oldEndPoint, SocketEndPoint newEndPoint)
            {
                _oldEndPoint = oldEndPoint;
                _newEndPoint = newEndPoint;
            }

            /// <summary>
            /// The previous endpoint.
            /// </summary>
            public SocketEndPoint OldEndPoint => _oldEndPoint;

            /// <summary>
            /// The new current endpoint.
            /// </summary>
            public SocketEndPoint NewEndPoint => _newEndPoint;
        }
        /// <summary>
        /// Values provided when a message is received from a server. 
        /// </summary>
        public class MessageReceivedEventArgs : EventArgs
        {
            private readonly object[] _parameters;
            private readonly long _messageId;
            private readonly DateTime _timeoutAtServerUTC;

            internal MessageReceivedEventArgs(object[] Parameters, long messageId, DateTime timeoutAtServerUTC)
            {
                _parameters = Parameters;
                _messageId = messageId;
                _timeoutAtServerUTC = timeoutAtServerUTC;
            }

            /// <summary>
            /// The message identifier. This is a unique identifier for the message.
            /// </summary>
            public long MessageId => _messageId;

            /// <summary>
            /// The parameters provided with the message.
            /// </summary>
            public object[] Parameters => _parameters;

            /// <summary>
            /// The byte array which can optionally be returned to the client. Null is returned if a 'Response' value is not provided.
            /// </summary>
            public byte[] Response { get; set; } = null;

            /// <summary>
            /// The UTC datetime when the message will timeout at the server.
            /// </summary>
            public DateTime TimeoutAtServerUTC => _timeoutAtServerUTC;
        }


        /// <summary>
        /// Values provided when a broadcast is received from the socket server. 
        /// </summary>
        public class BroadcastReceivedEventArgs : EventArgs
        {
            private readonly string _name;
            private readonly object[] _parameters;

            internal BroadcastReceivedEventArgs(string Name, object[] Parameters)
            {
                _name = Name;
                _parameters = Parameters;
            }

            /// <summary>
            /// The parameters provided with the message.
            /// </summary>
            public object[] Parameters => _parameters;

            /// <summary>
            /// Optional Name/Tag/Identifier for the broadcast 
            /// </summary>
            public string Name => _name;
        }


    }

}

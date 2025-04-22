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

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable CA1805 // Do not initialize unnecessarily
#pragma warning disable CA1819 // Properties should not return arrays
#pragma warning disable CA1034 // Nested types should not be visible

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
        /// Values provided when a message is received from a server. 
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


            /// <summary>
            /// The byte array which can optionally be returned to the client. Null is returned if a 'Response' value is not provided.
            /// </summary>
            public byte[] Response { get; set; } = null;
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
            public object[] Parameters { get { return _parameters; } }

            /// <summary>
            /// Optional Name/Tag/Identifier for the broadcast 
            /// </summary>
            public string Name {  get { return _name; } }
        }


    }

}

#pragma warning restore CA1034 // Nested types should not be visible
#pragma warning restore CA1819 // Properties should not return arrays
#pragma warning restore CA1805 // Do not initialize unnecessarily
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0079 // Remove unnecessary suppression


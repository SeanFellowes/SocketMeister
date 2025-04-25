#if !SMNOSERVER
using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister
{
#if SMISPUBLIC
    public partial class SocketServer
#else
    internal partial class SocketServer
#endif
    {
        /// <summary>
        /// Provides information for client-specific events.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Will be resolved in a future version")]
        public class ClientEventArgs : EventArgs
        {
            internal ClientEventArgs(Client client)
            {
                this.Client = client;
            }

            /// <summary>
            /// Gets the client that triggered the event.
            /// </summary>
            public Client Client { get; }
        }

        /// <summary>
        /// Provides values when a request is received from a client.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Will be resolved in a future version")]
        public class MessageReceivedEventArgs : EventArgs
        {
            internal MessageReceivedEventArgs(Client client, object[] parameters)
            {
                this.Client = client;
                this.Parameters = parameters;
            }

            /// <summary>
            /// Gets or sets the byte array to be returned to the client. If a 'Response' value is not provided when processing the 'MessageReceived' event, null is returned.
            /// </summary>
            public byte[] Response { get; set; } = null;

            /// <summary>
            /// Gets the client that initiated the message.
            /// </summary>
            public Client Client { get; private set; }

            /// <summary>
            /// Gets the parameters provided with the message.
            /// </summary>
            public object[] Parameters { get; private set; }
        }
    }
}

#endif

﻿#if !SMNOSERVER && !NET35
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
        /// Provided for client specific events.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Will be resolved in future version")]
        public class ClientEventArgs : EventArgs
        {
            internal ClientEventArgs(Client Client)
            {
                this.Client = Client;
            }

            /// <summary>
            /// The client which connected
            /// </summary>
            public Client Client { get; }
        }


        /// <summary>
        /// Values provided when a request is received from a client. 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Will be resolved in future version")]
        public class MessageReceivedEventArgs : EventArgs
        {
            internal MessageReceivedEventArgs(Client Client, object[] Parameters)
            {
                this.Client = Client;
                this.Parameters = Parameters;
            }

            /// <summary>
            /// The byte array which is to be returned to the client. Null is returned if a 'Response' value is not provided when processing the 'MessageReceived' event.
            /// </summary>
            public byte[] Response { get; set; } = null;

            /// <summary>
            /// The client which initiated the message.
            /// </summary>
            public Client Client { get; private set; }

            /// <summary>
            /// The parameters provided with the message.
            /// </summary>
            public object[] Parameters { get; private set; }

        }
    }
}

#endif

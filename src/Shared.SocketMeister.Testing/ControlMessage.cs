using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister.Testing
{
    static class ControlMessage
    {
        /// <summary>
        /// A socket which allows control messages to flow between a test client and test server has connected.
        /// </summary>
        public const int ClientControllerConnected = 1;

        /// <summary>
        /// Test harness sends this to a test client when it wants the client to disconnect and the client app to exit.
        /// </summary>
        public const int ExitClient = 2;

        ///// <summary>
        ///// When a test client receives a DisconnectClient message is sends the server a ClientDisconnecting message before the client app closes.
        ///// </summary>
        //public const int ClientDisconnecting = 3;
    }
}

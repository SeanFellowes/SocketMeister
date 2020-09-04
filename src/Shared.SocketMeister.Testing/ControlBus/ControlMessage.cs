using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister.Testing.ControlBus
{
    static class ControlMessage
    {
        /// <summary>
        /// A harness control bus socket client (ServerController or ClientController) is connecting to the HarnessController.
        /// </summary>
        public const int HarnessControlBusClientIsConnecting = 1;

        /// <summary>
        /// Test harness sends this to a test client when it wants the client to disconnect and the client app to exit.
        /// </summary>
        public const int ExitClient = 2;


        public const int SocketServerStart = 100;
        public const int SocketServerStop = 101;

        public const int SocketClientStart = 200;
        public const int SocketClientStop = 201;



        ///// <summary>
        ///// When a test client receives a DisconnectClient message is sends the server a ClientDisconnecting message before the client app closes.
        ///// </summary>
        //public const int ClientDisconnecting = 3;
    }
}

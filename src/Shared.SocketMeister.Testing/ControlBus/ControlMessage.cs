using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister.Testing.ControlBus
{
    public class ControlMessage
    {
        /// <summary>
        /// A harness control bus socket client (ServerController or ClientController) is connecting to the HarnessController.
        /// </summary>
        public static readonly short HarnessControlBusClientIsConnecting = 1;

        public static readonly short SocketServerStart = 100;
        public static readonly short SocketServerStop = 101;

        public static readonly short SocketClientStart = 200;
        public static readonly short SocketClientStop = 201;

        public static readonly short ExecuteCommand = 1000;

        /// <summary>
        /// Test harness sends this to a test client when it wants the client to disconnect and the client app to exit.
        /// </summary>
        public static readonly short ExitClient = short.MaxValue;
    }

}

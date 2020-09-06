using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister.Testing.ControlBus
{
    public enum ControlMessage
    {
        /// <summary>
        /// A harness control bus socket client (ServerController or ClientController) is connecting to the HarnessController.
        /// </summary>
        HarnessControlBusClientIsConnecting = 1,

        SocketServerStart = 100,
        SocketServerStop = 101,

        SocketClientStart = 200,
        SocketClientStop = 201,

        /// <summary>
        /// Test harness sends this to a test client when it wants the client to disconnect and the client app to exit.
        /// </summary>
        ExitClient = short.MaxValue
    }

}

using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister.Testing
{
    /// <summary>
    /// Commands executed on the client, via the ControlBus 
    /// </summary>
    internal static class ClientControllerCommands
    {
        internal static void ClientToServerSendRequestEcho01(ClientController Controller, int TransactionId, int MessageLength, int TimeoutMilliseconds = 30000)
        {
            object[] p = new object[4];
            //  MANDATORY FOR ALL
            p[0] = TransactionId;
            p[1] = nameof(ClientToServerSendRequestEcho01);
            //  SPECIFIC TO THIS
            p[2] = MessageLength;
            p[3] = new string('*', MessageLength);
            byte[] rVal = Controller.SocketClient.SendMessage(p, TimeoutMilliseconds);
        }
    }
}

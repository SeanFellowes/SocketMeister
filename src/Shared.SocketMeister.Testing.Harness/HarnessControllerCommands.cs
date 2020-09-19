using SocketMeister.Testing.ControlBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SocketMeister.Testing
{
    /// <summary>
    /// Commands executed on the server, via the ControlBus 
    /// </summary>
    internal static class HarnessControllerCommands
    {
        internal static void ClientToServerSendRequestEcho01(HarnessServerController Server, HarnessClientController Client)
        {
            //  START SOCKET SERVER IN BACHGROUND
            //Thread bgServer = new Thread(new ThreadStart(delegate
            //{
            //}));
            //bgServer.IsBackground = true;
            //bgServer.Start();

            //ThreadPool.QueueUserWorkItem(state =>
            //{
            //    try
            //    {
            //        action();
            //    }
            //    catch (Exception ex)
            //    {
            //        OnException(ex);
            //    }
            //});

            //  SEND A 1KB FILE
            byte[] rVal = Client.Commands.ExecuteMethod(nameof(ClientControllerCommands), nameof(ClientControllerCommands.ClientToServerSendRequestEcho01));
        }

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SocketMeister.Testing
{
    /// <summary>
    /// Test Harness Client
    /// </summary>
    internal class ClientController
    {
        private readonly HarnessControlBusClient _harnessControlBusClient;
        private readonly object _lock = new object();

        /// <summary>
        /// Triggered when connection could not be established with the HarnessController. This ClientController should now abort (close)
        /// </summary> 
        public event EventHandler<EventArgs> HarnessConnectionFailed;

        public ClientController(int HarnessControlBusClientId, string HarnessControlBusIPAddress)
        {
            _harnessControlBusClient = new HarnessControlBusClient( HarnessControlBusClientType.ClientController, HarnessControlBusClientId, HarnessControlBusIPAddress, Constants.HarnessControlBusPort);
            _harnessControlBusClient.ConnectionFailed += harnessControlBusClient_ConnectionFailed;
            _harnessControlBusClient.HarnessControlBusSocketClient.MessageReceived += HarnessControlBusSocketClient_MessageReceived;
        }

        private void HarnessControlBusSocketClient_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void harnessControlBusClient_ConnectionFailed(object sender, EventArgs e)
        {
            //  CONNECTION TO THE HarnessController COULD NOT BE ESTABLISHED.
            HarnessConnectionFailed?.Invoke(this, e);
        }


        public void Stop()
        {
            _harnessControlBusClient.Stop();
        }

    }
}

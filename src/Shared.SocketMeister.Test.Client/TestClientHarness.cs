using System;
using System.Collections.Generic;
using System.Text;

namespace SocketMeister.Test
{
    public class TestClientHarness
    {
        private readonly SocketClient controlSocket = null;


        public TestClientHarness()
        {
            List<SocketEndPoint> endPoints = new List<SocketEndPoint>() { new SocketEndPoint("127.0.0.0", 4505) };
            controlSocket = new SocketClient(endPoints);
            controlSocket.ConnectionStatusChanged += controlSocket_ConnectionStatusChanged;
        }

        private void controlSocket_ConnectionStatusChanged(object sender, SocketClient.ConnectionStatusChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

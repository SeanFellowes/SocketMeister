using System;
using System.Windows.Forms;
using SocketMeister;

namespace SocketMeisterDemo
{
    public partial class SimpleServer : Form
    {
        //  Instatiate SocketServer on port 4505 with compression enabled
        private readonly SocketServer _socketServer = new SocketServer(4505, true);

        public SimpleServer()
        {
            InitializeComponent();

            //  Listen to RequestReceived events
            _socketServer.RequestReceived += SocketServer_RequestReceived;

            //  Start the socket server
            _socketServer.Start();
        }
		private void SocketServer_RequestReceived(object sender, SocketServer.RequestReceivedEventArgs e)
		{
			//  Example request
			if (Convert.ToString(e.Parameters[0]) == "GetTimezoneDisplayName")
			{
				//  Response is a binary array. Convert string to binary array.
				e.Response = System.Text.Encoding.ASCII.GetBytes(TimeZoneInfo.Local.DisplayName);
			}
		}

        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
			//  Stop the socket server before exiting the application
			_socketServer.Stop();
		}

        private void btnSendMsgToAllClients_Click(object sender, EventArgs e)
        {
            //  Send a string to all connected clients
            object[] parms = new object[] { "This is a broadcast message to all clients" };
            _socketServer.BroadcastMessage(parms);
        }

        private void btnSendMsgToAllCustomerDataChangeSubscribers_Click(object sender, EventArgs e)
        {
            //  Send a string to all clients subscribing to 'Customer Master Data Changes'
            object[] parms = new object[] { "This is a broadcast message to all clients subscribing to 'Customer Master Data Changes'" };
            _socketServer.BroadcastMessageToSubscribers("Customer Master Data Changes", parms);
        }


    }
}

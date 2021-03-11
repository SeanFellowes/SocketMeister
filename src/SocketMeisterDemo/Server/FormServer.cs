using System;
using System.Windows.Forms;
using SocketMeister;

namespace SocketMeisterDemo
{
    public partial class FormServer : Form
    {
        //  Instatiate SocketServer on port 4505 with compression enabled
        private readonly SocketServer _socketServer = new SocketServer(4505, true);

        public FormServer()
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
	}
}

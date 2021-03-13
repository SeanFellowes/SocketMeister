using System;
using System.Windows.Forms;
using SocketMeister;

namespace SocketMeisterDemo
{
    public partial class SimpleClient : Form
    {
        //  Start socket client, pointed at 127.0.0.1:4505 with compression enabled (127.0.0.1 is localhost)
        SocketClient _socketClient = new SocketClient("127.0.0.1", 4505, true);

        public SimpleClient()
        {
            InitializeComponent();

            //  Register to receive messages from the server.
            _socketClient.MessageReceived += SocketClient_MessageReceived;

            //  Subscribe to 'Customer Master Data Changes'. (Note: names are case insensitive)
            _socketClient.AddSubscription("Customer Master Data Changes");
        }

        private void SocketClient_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            //  The message is a string contained in the first parameter sent. Display it.
            MessageBox.Show(Convert.ToString(e.Parameters[0]));
        }

        private void BtnGetTimezoneDisplayName_Click(object sender, EventArgs e)
        {
            object[] parms = new object[] { "GetTimezoneDisplayName" };
            byte[] response =_socketClient.SendRequest(parms);

            //  Response is a binary array. Convert it to a string and display the message.
            MessageBox.Show(System.Text.Encoding.ASCII.GetString(response));
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //  Disconnect from the socket server
            _socketClient.Stop();
        }
    }
}

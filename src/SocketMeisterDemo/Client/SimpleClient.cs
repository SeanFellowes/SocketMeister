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

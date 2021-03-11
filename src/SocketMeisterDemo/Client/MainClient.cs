using System;
using System.Windows.Forms;
using SocketMeister;

namespace Client
{
    public partial class ClientForm : Form
    {
        //  Start socket client, pointed at 127.0.0.1:4505 with compression enabled (127.0.0.1 is localhost)
        SocketClient _socketClient = new SocketClient("127.0.0.1", 4505, true);

        public ClientForm()
        {
            InitializeComponent();
        }

        private void BtnGetTimezoneDisplayName_Click(object sender, EventArgs e)
        {
            object[] parms = new object[] { "GetTimezoneDisplayName" };
            byte[] result =_socketClient.SendRequest(parms);
            MessageBox.Show(System.Text.Encoding.ASCII.GetString(result));
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _socketClient.Stop();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SocketMeister;

namespace Test.Server
{
    public partial class Server : UserControl
    {
        //  Silverlight ports are between 4502-4534
        private int _port = 4502;
        private SocketServer _socketServer;

        public Server()
        {
            InitializeComponent();
            LabelPort.Text = "Port " + _port.ToString();
        }

        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                LabelPort.Text = "Port " + _port.ToString();
            }
        }

        private void StartStopButton_Click(object sender, EventArgs e)
        {
            _socketServer = new SocketServer(_port, true);
        }
    }
}

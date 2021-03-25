using System;
using System.Drawing;
using System.Windows.Forms;

namespace SocketMeister.Test
{
    public partial class SocketServerOverview : UserControl
    {
        /// <summary>
        /// Type of socket server
        /// </summary>
        public enum SocketServerTypes
        {
            /// <summary>
            /// Standard socket server
            /// </summary>
            SocketServer = 0,
            /// <summary>
            /// Policy server
            /// </summary>
            PolicyServer = 1,
            /// <summary>
            /// Socketserver for the control bus
            /// </summary>
            ControlServer = 2
        }

        private int _port = 4502;
        private readonly object _lock = new object();
        private SocketServerTypes _socketServerType = SocketServerTypes.SocketServer;

        public SocketServerOverview()
        {
            InitializeComponent();
            RefreshUserInterface();
        }




        public int Port
        {
            get
            {
                lock (_lock) { return _port; }
            }
            set
            {
                lock (_lock) { _port = value; }
                RefreshUserInterface();
            }
        }

        public SocketServerTypes SocketServerType
        {
            get
            {
                lock (_lock) { return _socketServerType; }
            }
            set
            {
                lock (_lock) { _socketServerType = value; }
                RefreshUserInterface();
            }
        }

        public void SetClientCount(int ClientCount)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { SetClientCount(ClientCount); }));
            else
            {
                ClientCountLabel.Text = ClientCount.ToString();
            }
        }


        public void SetListenerState(object sender, EventArgs e)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { SetListenerState(sender, e); }));
            else
            {
                Testing.ControlBus.ControlBusServer serverCtl = (Testing.ControlBus.ControlBusServer)sender;
                if (serverCtl.Listener.Status == SocketServerStatus.Started)
                {
                    LabelStatus.Text = "Started";
                    StatusIndicator.BackColor = Color.DarkGreen;
                    Cursor = Cursors.Default;
                }
                else if (serverCtl.Listener.Status == SocketServerStatus.Starting)
                {
                    LabelStatus.Text = "Starting...";
                    StatusIndicator.BackColor = Color.Yellow;
                }
                else if (serverCtl.Listener.Status == SocketServerStatus.Stopped)
                {
                    LabelStatus.Text = "Stopped";
                    StatusIndicator.BackColor = Color.Red;
                    Cursor = Cursors.Default;
                }
                else
                {
                    LabelStatus.Text = "Stopping...";
                    StatusIndicator.BackColor = Color.DarkOrange;
                }
            }
        }



        private void RefreshUserInterface()
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { RefreshUserInterface(); }));
            else
            {
                if (SocketServerType == SocketServerTypes.PolicyServer)
                {
                    LabelPort.Text = "Policy server on port 943";
                    ClientCountIcon.Visible = false;
                    ClientCountLabel.Visible = false;
                }
                else
                {
                    LabelPort.Text = "Socket server on port " + _port.ToString();
                    ClientCountIcon.Visible = true;
                    ClientCountLabel.Visible = true;
                }
            }
        }


    }


}

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
        private int _clientCount = 0;
        private SocketServerTypes _socketServerType = SocketServerTypes.SocketServer;

        public SocketServerOverview()
        {
            InitializeComponent();
            RefreshUserInterface();
        }


        public int ClientCount
        {
            get
            {
                return _clientCount;
            }
            set
            {
                _clientCount = value;
                ClientCountLabel.Text = value.ToString();
            }
        }


        public int Port
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;
                RefreshUserInterface();
            }
        }

        public SocketServerTypes SocketServerType
        {
            get
            {
                return _socketServerType;
            }
            set
            {
                _socketServerType = value;
                RefreshUserInterface();
            }
        }



        public void SetListenerState(SocketServerStatus Status)
        {
            try
            {
                if (InvokeRequired) Invoke(new MethodInvoker(delegate { SetListenerState(Status); }));
                else
                {
                    if (Status == SocketServerStatus.Started)
                    {
                        LabelStatus.Text = "Started";
                        StatusIndicator.BackColor = Color.DarkGreen;
                        this.Cursor = Cursors.Default;
                    }
                    else if (Status == SocketServerStatus.Starting)
                    {
                        LabelStatus.Text = "Starting...";
                        StatusIndicator.BackColor = Color.Yellow;
                    }
                    else if (Status == SocketServerStatus.Stopped)
                    {
                        LabelStatus.Text = "Stopped";
                        StatusIndicator.BackColor = Color.Red;
                        this.Cursor = Cursors.Default;
                    }
                    else
                    {
                        LabelStatus.Text = "Stopping...";
                        StatusIndicator.BackColor = Color.DarkOrange;
                    }
                }
            }
            catch { }
        }




        private void RefreshUserInterface()
        {
            if (_socketServerType == SocketServerTypes.PolicyServer)
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

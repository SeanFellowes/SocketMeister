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
    public enum ServerType
    {
        SocketServer = 0,
        PolicyServer = 1
    }

    public partial class Server : UserControl
    {
        //  Silverlight ports are between 4502-4534
        private const int policyPort = 943;
        private PolicyServer policyServer = null;
        private int port = 4502;
        private ServerType serverType = ServerType.SocketServer;
        private SocketServer socketServer = null;

        /// <summary>
        /// Raised when an trace log event has been raised.
        /// </summary>
        internal event EventHandler<TraceEventArgs> TraceEventRaised;



        public Server()
        {
            InitializeComponent();
            SetLabel();
        }

        public int Port
        {
            get
            {
                if (serverType == ServerType.PolicyServer) return policyPort;
                return port;
            }
            set
            {
                port = value;
                SetLabel();
            }
        }

        public ServerType ServerType
        {
            get { return serverType; }
            set
            {
                serverType = value;
                SetLabel();
                if (serverType == ServerType.PolicyServer)
                {
                    SessionCountIcon.Visible = false;
                    SessionCountLabel.Visible = false;
                }
                else
                {
                    SessionCountIcon.Visible = true;
                    SessionCountLabel.Visible = true;
                }
            }
        }

        public ServiceStatus Status
        {
            get
            {
                if (policyServer != null) return policyServer.Status;
                else if (socketServer != null) return socketServer.Status;
                else return ServiceStatus.Stopped;
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            Stop();
        }


        private void SocketServer_ListenerStateChanged(object sender, SocketServer.ServerStatusEventArgs e)
        {
            ChangeStatus(e.Status);
        }


        private void PolicyServer_SocketServiceStatusChanged(object sender, PolicyServer.ServerStatusEventArgs e)
        {
            ChangeStatus(e.Status);
        }


        private void ChangeStatus(ServiceStatus Status)
        {
            try
            {
                if (InvokeRequired) Invoke(new MethodInvoker(delegate { ChangeStatus(Status); }));
                else
                {
                    if (Status == ServiceStatus.Started)
                    {
                        LabelStatus.Text = "Started";
                        StatusIndicator.BackColor = Color.DarkGreen;
                        this.Cursor = Cursors.Default;
                    }
                    else if (Status == ServiceStatus.Starting)
                    {
                        LabelStatus.Text = "Starting...";
                        StatusIndicator.BackColor = Color.Yellow;
                    }
                    else if (Status == ServiceStatus.Stopped)
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        /// <summary>
        /// Attempts to stop the service.
        /// </summary>
        /// <returns>True if an attempt to stop occured, False is the service is not running</returns>
        public bool Stop()
        {
            try
            {
                if (socketServer == null) return false;
                if (socketServer.Status == ServiceStatus.Started)
                {
                    this.Cursor = Cursors.WaitCursor;
                    socketServer.Stop();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Stopping", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }


        private void SetLabel()
        {
            if (serverType == ServerType.SocketServer) LabelPort.Text = "Socket server on port " + port.ToString();
            else LabelPort.Text = "Policy server on port " + policyPort.ToString();
        }

        public void Start()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                Application.DoEvents();

                //  START IN THE BACKGROUND
                BackgroundWorker bgStartService = new BackgroundWorker();
                bgStartService.DoWork += bgStartService_DoWork;
                bgStartService.RunWorkerCompleted += bgStartService_RunWorkerCompleted;
                bgStartService.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                string msg = ex.Message;
                if (ex.StackTrace != null) msg += "\n\n" + ex.StackTrace;
                MessageBox.Show(msg);
            }
        }



        private void bgStartService_DoWork(object sender, DoWorkEventArgs e)
        {
            if (serverType == ServerType.SocketServer)
            {
                socketServer = new SocketServer(port, true);
                socketServer.ListenerStateChanged += SocketServer_ListenerStateChanged;
                socketServer.TraceEventRaised += Server_TraceEventRaised;
                socketServer.Start();
            }
            else
            {
                policyServer = new PolicyServer();
                policyServer.SocketServiceStatusChanged += PolicyServer_SocketServiceStatusChanged;
                policyServer.TraceEventRaised += Server_TraceEventRaised;
                policyServer.Start();
            }
        }

        private void Server_TraceEventRaised(object sender, TraceEventArgs e)
        {
            TraceEventRaised?.Invoke(this, e);
        }

        private void bgStartService_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { bgStartService_RunWorkerCompleted(sender, e); }));
            else
            {
                this.Cursor = Cursors.Default;
                if (e.Error != null)
                {
                    string msg = e.Error.Message;
                    if (e.Error.StackTrace != null) msg += "\n\n" + e.Error.StackTrace;
                    MessageBox.Show(msg, "Error Starting", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (e.Cancelled == true)
                {
                }
            }
        }

    }
}

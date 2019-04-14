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
    public enum ServerType
    {
        SocketServer = 0,
        PolicyServer = 1
    }

    public partial class Server : UserControl
    {
        //  Silverlight ports are between 4502-4534
        private const int _policyPort = 943;
        private PolicyServer _policyServer = null;
        private int _port = 4502;
        private ServerType _serverType = ServerType.SocketServer;
        private SocketServer _socketServer = null;

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
            get { return _port; }
            set
            {
                _port = value;
                SetLabel();
            }
        }

        public ServerType ServerType
        {
            get { return _serverType; }
            set
            {
                _serverType = value;
                SetLabel();
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
                if (_socketServer == null) return false;
                if (_socketServer.ListenerState == ServiceStatus.Started)
                {
                    this.Cursor = Cursors.WaitCursor;
                    _socketServer.Stop();
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
            if (_serverType == ServerType.SocketServer) LabelPort.Text = "Socket server on port " + _port.ToString();
            else LabelPort.Text = "Policy server on port " + _policyPort.ToString();
        }

        private void Start()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                Application.DoEvents();

                //  START IN THE BACKGROUND
                BackgroundWorker bgStartService = new BackgroundWorker();
                bgStartService.DoWork += _bgStartService_DoWork;
                bgStartService.RunWorkerCompleted += _bgStartService_RunWorkerCompleted;
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



        private void _bgStartService_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_serverType == ServerType.SocketServer)
            {
                _socketServer = new SocketServer(_port, true);
                _socketServer.ListenerStateChanged += SocketServer_ListenerStateChanged;
                _socketServer.TraceEventRaised += Server_TraceEventRaised;
                _socketServer.Start();
            }
            else
            {
                _policyServer = new PolicyServer();
                _policyServer.SocketServiceStatusChanged += PolicyServer_SocketServiceStatusChanged;
                _policyServer.TraceEventRaised += Server_TraceEventRaised;
                _policyServer.Start();
            }
        }

        private void Server_TraceEventRaised(object sender, TraceEventArgs e)
        {
            TraceEventRaised?.Invoke(this, e);
        }

        private void _bgStartService_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { _bgStartService_RunWorkerCompleted(sender, e); }));
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

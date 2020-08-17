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
        public const int PolicyPort = 943;

        private readonly object lockControl = new object();
        private int port = 4502;
        private ServerType serverType = ServerType.SocketServer;
        private SocketServer socketServer = null;

        /// <summary>
        /// Raised when an trace log event has been raised.
        /// </summary>
        internal event EventHandler<TraceEventArgs> TraceEventRaised;

        /// <summary>
        /// Raised when a message is received from a client.
        /// </summary>
        internal event EventHandler<SocketServer.MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Raised when a request message is received from a client. A response can be provided which will be returned to the client.
        /// </summary>
        internal event EventHandler<SocketServer.RequestReceivedEventArgs> RequestReceived;



        public Server()
        {
            InitializeComponent();
            SetLabel();
        }


        /// <summary>
        /// When parent form is closing, unregister events to stop errors occuring when socket shutdown events bubble up to the non-existent parent
        /// </summary>
        public void UnregisterEvents()
        {
            if (socketServer != null)
            {
                socketServer.ClientsChanged -= SocketServer_ClientsChanged;
                socketServer.ListenerStateChanged -= SocketServer_ListenerStateChanged;
                socketServer.TraceEventRaised -= SocketServer_TraceEventRaised;
                socketServer.MessageReceived -= SocketServer_MessageReceived;
                socketServer.RequestReceived -= SocketServer_RequestReceived;
            }
        }

        public SocketServer SocketServer
        {
            get { return socketServer; }
            set { socketServer = value; }
        }

        public int Port
        {
            get
            {
                //if (serverType == ServerType.PolicyServer) return policyPort;
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

        //public SocketServerStatus Status
        //{
        //    get
        //    {
        //        if (policyServer != null) return policyServer.Status;
        //        else if (socketServer != null) return socketServer.Status;
        //        else return SocketServerStatus.Stopped;
        //    }
        //}

        private void BtnStart_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            Stop();
        }


        private void SocketServer_ListenerStateChanged(object sender, SocketServer.SocketServerStatusChangedEventArgs e)
        {
            ChangeStatus(e.Status);
        }



        private void ChangeStatus(SocketServerStatus Status)
        {
            try
            {
                if (InvokeRequired) Invoke(new MethodInvoker(delegate { ChangeStatus(Status); }));
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



        /// <summary>
        /// Attempts to stop the service.
        /// </summary>
        /// <returns>True if an attempt to stop occured, False is the service is not running</returns>
        public bool Stop()
        {
            try
            {
                if (ServerType == ServerType.SocketServer)
                {
                    if (socketServer == null) return false;
                    if (socketServer.Status ==  SocketServerStatus.Started)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        socketServer.Stop();
                        return true;
                    }
                    return false;
                }
                else
                {
                    //if (policyServer == null) return false;
                    //if (policyServer.Status == SocketServerStatus.Started)
                    //{
                    //    this.Cursor = Cursors.WaitCursor;
                    //    policyServer.Stop();
                    //    return true;
                    //}
                    return false;
                }
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
            else LabelPort.Text = "Policy server on port " + PolicyPort.ToString();
        }

        public void Start()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                Application.DoEvents();

                //  START IN THE BACKGROUND
                BackgroundWorker bgStartService = new BackgroundWorker();
                bgStartService.DoWork += BgStartService_DoWork;
                bgStartService.RunWorkerCompleted += BgStartService_RunWorkerCompleted;
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



        private void BgStartService_DoWork(object sender, DoWorkEventArgs e)
        {
            if (serverType == ServerType.SocketServer)
            {
                socketServer = new SocketServer(port, true);
                socketServer.ClientsChanged += SocketServer_ClientsChanged;
                socketServer.ListenerStateChanged += SocketServer_ListenerStateChanged;
                socketServer.TraceEventRaised += SocketServer_TraceEventRaised;
                socketServer.MessageReceived += SocketServer_MessageReceived;
                socketServer.RequestReceived += SocketServer_RequestReceived;
                socketServer.Start();
            }
            //else
            //{
            //    policyServer.Start();
            //}
        }

        private void SocketServer_MessageReceived(object sender, SocketServer.MessageReceivedEventArgs e)
        {
            MessageReceived?.Invoke(sender, e);
        }

        private void SocketServer_RequestReceived(object sender, SocketServer.RequestReceivedEventArgs e)
        {
            RequestReceived?.Invoke(sender, e);
        }

        private void SocketServer_ClientsChanged(object sender, SocketServer.ClientsChangedEventArgs e)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { SocketServer_ClientsChanged(sender, e); }));
            else
            {
                SessionCountLabel.Text = e.Count.ToString();
            }
        }

        private void SocketServer_TraceEventRaised(object sender, TraceEventArgs e)
        {
            TraceEventRaised?.Invoke(this, e);
        }

        private void BgStartService_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (InvokeRequired) Invoke(new MethodInvoker(delegate { BgStartService_RunWorkerCompleted(sender, e); }));
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

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
    public partial class PolicyServerOverview : UserControl
    {
        public const int Port = 943;

        private readonly object lockControl = new object();
        private PolicyServer policyServer;

        /// <summary>
        /// Raised when an trace log event has been raised.
        /// </summary>
        internal event EventHandler<TraceEventArgs> TraceEventRaised;

        public PolicyServerOverview()
        {
            InitializeComponent();
        }

        public PolicyServer PolicyServer
        {
            get { return policyServer; }
            set 
            {
                if (value == null) return;
                if (policyServer != null) throw new ApplicationException(nameof(PolicyServer) + " can only be set once");

                policyServer = value;
                policyServer.TraceEventRaised += PolicyServer_TraceEventRaised;
                policyServer.SocketServerStatusChanged += PolicyServer_SocketServerStatusChanged;
            }
        }

        private void PolicyServer_SocketServerStatusChanged(object sender, PolicyServer.ServerStatusEventArgs e)
        {
            ChangeStatus(e.Status);
        }

        private void PolicyServer_TraceEventRaised(object sender, TraceEventArgs e)
        {
            TraceEventRaised?.Invoke(sender, e);
        }



        private void BtnStart_Click(object sender, EventArgs e)
        {
            Start();
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
            policyServer.Start();
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SocketMeister
{
    delegate void SetLabelTextDelegate(Label label, string text);
    delegate void ListBoxAddItemDelegate(GridItem.SeverityType Severity, string Source, string Text);
    delegate void SetButtonEnabledDelegate(Button button, bool enabled);
    delegate void SetGroupBoxEnabledDelegate(GroupBox groupbox, bool enabled);

    public partial class Main : Form
    {
        BindingList<GridItem> _gridItems;
        private readonly object _lock = new object();
        private bool _stop = false;

        public Main()
        {
            InitializeComponent();
            this.Top = 0;
            this.Left = 0;
        }

        private void Server_ListenerStateChanged(object sender, SocketServer.SocketServerStatusChangedEventArgs e)
        {
            SetUI(e.Status);
        }

        private void Server_ExceptionRaised(object sender, ExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                InsertListboxItem(GridItem.SeverityType.Error, "SocketServer", e.Exception.Message);
            }
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            dGrid.AutoGenerateColumns = true;
            _gridItems = new BindingList<GridItem>();
            _gridItems.AllowNew = true;
            _gridItems.AllowRemove = true;
            _gridItems.RaiseListChangedEvents = true;
            _gridItems.AllowEdit = false;
            dGrid.DataSource = _gridItems;

            Start();
        }


        private void Server_RequestReceived(object sender, SocketServer.RequestReceivedEventArgs e)
        {
            SetLabelText(lblTotalRequestsReceived, Global.Server.TotalRequestsReceived.ToString("N0"));
            SetLabelText(lblBytesReceived, Global.Server.TotalBytesReceived.ToString("N0"));

            //MESSAGE RECEIVED. SEND IT BACK IF LOGGING IS ON
            int clientId = (int)e.Parameters[0];
            byte[] receivedBytes = (byte[])e.Parameters[1];
            string msgRec = Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);

            InsertListboxItem(GridItem.SeverityType.Information, "Client " + clientId, "RequestReceived: " + msgRec);

            byte[] toSend = new byte[msgRec.Length];
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(msgRec), 0, toSend, 0, toSend.Length);
            e.Response = receivedBytes;
        }


        void Server_MessageReceived(object sender, SocketServer.MessageReceivedEventArgs e)
        {
            SetLabelText(lblTotalRequestsReceived, Global.Server.TotalRequestsReceived.ToString("N0"));
            SetLabelText(lblBytesReceived, Global.Server.TotalBytesReceived.ToString("N0"));

            //MESSAGE RECEIVED. SEND IT BACK IF LOGGING IS ON
            int clientId = (int)e.Parameters[0];
            byte[] receivedBytes = (byte[])e.Parameters[1];
            string msgRec = Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);

            InsertListboxItem(GridItem.SeverityType.Information, "Client " + clientId, "MessageReceived: " + msgRec);
        }



        private void SetButtonEnabled(Button button, bool enabled)
        {
            if (button.InvokeRequired)
            {
                SetButtonEnabledDelegate d = new SetButtonEnabledDelegate(SetButtonEnabled);
                this.Invoke(d, new object[] { button, enabled });
            }
            else { button.Enabled = enabled; }
        }
        private void SetGroupBoxEnabled(GroupBox groupbox, bool enabled)
        {
            if (groupbox.InvokeRequired)
            {
                SetGroupBoxEnabledDelegate d = new SetGroupBoxEnabledDelegate(SetGroupBoxEnabled);
                this.Invoke(d, new object[] { groupbox, enabled });
            }
            else { groupbox.Enabled = enabled; }
        }
        private void InsertListboxItem(GridItem.SeverityType Severity, string Source, string Text)
        {
            try
            {
                if (this.dGrid.InvokeRequired)
                {
                    ListBoxAddItemDelegate d = new ListBoxAddItemDelegate(InsertListboxItem);
                    this.Invoke(d, new object[] { Severity, Source, Text });
                }
                else
                {
                    if (_gridItems.Count == 0) _gridItems.Add(new GridItem(Severity, Source, Text));
                    else _gridItems.Insert(0, new GridItem(Severity, Source, Text));

                }
            }
            catch { }
        }
        private void SetLabelText(Label label, string text)
        {
            if (label.InvokeRequired)
            {
                SetLabelTextDelegate d = new SetLabelTextDelegate(SetLabelText);
                this.Invoke(d, new object[] { label, text });
            }
            else { label.Text = text; }
        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void Start()
        {
            try
            {
                lock(_lock) { _stop = false; }

                //Global.Server.Start(tbIPAddress.Text, 100, cbCompressMessage.Checked, Convert.ToInt32(tbPort.Text), cbPolicyListener.Checked, cbEnableDiagnostics.Checked, cbEnableClientPinging.Checked);
                if (Global.Server != null)
                {
                    Global.Server.ClientConnected -= Server_ClientConnected;
                    Global.Server.ClientDisconnected -= Server_ClientDisconnected;
                    Global.Server.ExceptionRaised -= Server_ExceptionRaised;
                    Global.Server.MessageReceived -= Server_MessageReceived;
                    Global.Server.RequestReceived -= Server_RequestReceived;
                    Global.Server.ListenerStateChanged -= Server_ListenerStateChanged;
                }

                Global.Server = new SocketServer(Convert.ToInt32(tbPort.Text), cbCompressMessage.Checked);
                Global.Server.ClientConnected += Server_ClientConnected;
                Global.Server.ClientDisconnected += Server_ClientDisconnected;
                Global.Server.ExceptionRaised += Server_ExceptionRaised;
                Global.Server.MessageReceived += Server_MessageReceived;
                Global.Server.RequestReceived += Server_RequestReceived;
                Global.Server.ListenerStateChanged += Server_ListenerStateChanged;
                Global.Server.Start();

                btnStart.Enabled = false;       //  CHANGED WHEN EVENT IS RECEIVED
                pnlSettings.Enabled = false;

                //  START A BACKGROUND PROCESS WHICH AUTOMATICALLY GENERATES BROADCASTS
                new Thread(new ThreadStart(delegate { ExecuteBroadcaster(); })).Start();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                btnStart.Enabled = true;
            }
        }

        private void Server_ClientDisconnected(object sender, SocketServer.ClientDisconnectedEventArgs e)
        {
            SetLabelText(lblTotalConnectedClients, Global.Server.ClientCount.ToString("N0"));
        }

        private void Server_ClientConnected(object sender, SocketServer.ClientConnectedEventArgs e)
        {
            SetLabelText(lblTotalConnectedClients, Global.Server.ClientCount.ToString("N0"));
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                //  STOP
                lock (_lock) { _stop = true; }
                Global.Server.Stop();
                lblStatus.Text = "STOPPING ...";
                btnStop.Enabled = false;
                lblStatus.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "";
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            try { Global.Server.Stop(); }
            catch { }
            try { Global.Server = null; }
            catch { }
            Application.Exit();
        }



        private void btnBroadcast_Click(object sender, EventArgs e)
        {
            if (tbBroadcast.Text.Length == 0) { MessageBox.Show("Nothing to broadcast"); return; }

            byte[] toSend = new byte[tbBroadcast.Text.Length];
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(tbBroadcast.Text), 0, toSend, 0, toSend.Length);
            SendMessageToClients(toSend);
        }

        private readonly Random _rng = new Random();
        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string RandomString(int size)
        {
            char[] buffer = new char[size];

            for (int i = 0; i < size; i++)
            {
                buffer[i] = _chars[_rng.Next(_chars.Length)];
            }
            return new string(buffer);
        }

        private void SetUI(SocketServerStatus Status)
        {
            if (Status == SocketServerStatus.Started)
            {
                serviceStatusIndicator.BackColor = Color.Green;
                SetButtonEnabled(btnStart, false);
                SetButtonEnabled(btnStop, true);
                SetButtonEnabled(btnBroadcast, true);
                SetLabelText(lblStatus, "STARTED");
            }
            else if (Status == SocketServerStatus.Stopped)
            {
                serviceStatusIndicator.BackColor = Color.Red;
                SetButtonEnabled(btnStart, true);
                SetButtonEnabled(btnStop, false);
                SetButtonEnabled(btnBroadcast, false);
                SetGroupBoxEnabled(pnlSettings, true);
                SetLabelText(lblStatus, "STOPPED");
            }
            else
            {
                serviceStatusIndicator.BackColor = Color.Orange;
                SetButtonEnabled(btnStart, false);
                SetButtonEnabled(btnStop, false);
                SetButtonEnabled(btnBroadcast, false);
            }

        }

        private void ExecuteBroadcaster()
        {
            while (true == true)
            {
                lock(_lock) { if (_stop == true) return; }
                if (Global.Server == null)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    try
                    {
                        //  SEND A RANDOM MESSAGE OF A RANDOM SIZE (UP TO 1 MB)
                        if (Global.Server.Status == SocketServerStatus.Started)
                        {
                            if (rbAutoBCLarge.Checked == true)
                            {
                                string toSendStr = RandomString(_rng.Next(6048576) + 1000000);
                                byte[] toSend = new byte[toSendStr.Length];
                                Buffer.BlockCopy(Encoding.UTF8.GetBytes(toSendStr), 0, toSend, 0, toSend.Length);
                                SendMessageToClients(toSend);
                                Thread.Sleep(500);
                            }
                            else if (rbAutoBCFast.Checked == true)
                            {
                                string toSendStr = RandomString(_rng.Next(10000));
                                byte[] toSend = new byte[toSendStr.Length];
                                Buffer.BlockCopy(Encoding.UTF8.GetBytes(toSendStr), 0, toSend, 0, toSend.Length);
                                for (int i = 0; i < 10; i++)
                                {
                                    SendMessageToClients(toSend);
                                    Thread.Sleep(50);
                                }
                            }
                            else
                            {
                                Thread.Sleep(500);
                            }
                        }
                        else Thread.Sleep(500);

                    }
                    catch { Thread.Sleep(500); }
                }
            }
        }


        private void SendMessageToClients(byte[] Bytes)
        {
            object[] parms = new object[1];
            parms[0] = Bytes;

            List<SocketServer.Client> items = Global.Server.GetClients();
            foreach (SocketServer.Client i in items)
            {
                i.SendMessage(parms);
            }
            SetLabelText(lblTotalMessagesSent, Global.Server.TotalMessagesSent.ToString("N0"));
            SetLabelText(lblBytesSent, Global.Server.TotalBytesSent.ToString("N0"));

        }



    }

}

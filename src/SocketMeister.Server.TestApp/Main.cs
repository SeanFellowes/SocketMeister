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
    delegate void ListBoxAddItemDelegate(string source, string text, int clientID, int endPointID, int messageID);
    delegate void SetButtonEnabledDelegate(Button button, bool enabled);
    delegate void SetGroupBoxEnabledDelegate(GroupBox groupbox, bool enabled);

    public partial class Main : Form
    {
        BindingList<GridItem> _gridItems;

        public Main()
        {
            InitializeComponent();

            //  START A BACKGROUND PROCESS WHICH AUTOMATICALLY GENERATES BROADCASTS
            new Thread(new ThreadStart(delegate { ExecuteBroadcaster(); })).Start();

        }

        private void Server_ListenerStateChanged(object sender, SocketServer.SocketServerStatusChangedEventArgs e)
        {
            SetUI(e.Status);
        }

        private void Server_ExceptionRaised(object sender, ExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                InsertListboxItem("SocketServer", e.Exception.Message, 0, 0, 0);
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


        void socketServer_MessageReceived(object sender, SocketServer.MessageReceivedEventArgs e)
        {
            SocketServer.Client cli = (SocketServer.Client)sender;
            InsertListboxItem("Test App", "Message received from " + e.Client.ClientId, 0, 0, 0);
            //  MESSAGE RECEIVED. SEND IT BACK IF LOGGING IS ON
            //string msgRec = Encoding.UTF8.GetString(e.MessageBytes, 0, e.MessageBytes.Length);
            //byte[] toSend = new byte[msgRec.Length];
            //Buffer.BlockCopy(Encoding.UTF8.GetBytes(msgRec), 0, toSend, 0, toSend.Length);
            //cli.SendResponse(toSend, e.RequestID, e.EndPointID);
        }


        //  DIAGNOSTICS EVENTS
        //void diagnostics_BroadcastMessagesInQueueChanged(object sender, SockMstrCounterChangedArgs e)
        //{
        //    SetLabelText(lblOutboundMessageQueue, e.Count.ToString());
        //}
        //void diagnostics_BroadcastRecipientsChanged(object sender, SockMstrCounterChangedArgs e)
        //{
        //    SetLabelText(lblBroadcastRecipients, e.Count.ToString());
        //}
        //void diagnostics_CounterSubscriptionEndPointsChanged(object sender, SockMstrCounterChangedArgs e)
        //{
        //    SetLabelText(lblSubscriptionEndPoints, e.Count.ToString());
        //}
        //void diagnostics_CounterSubscriptionSubscribersChanged(object sender, SockMstrCounterSubscriptionSubscribersChangedArgs e)
        //{
        //    if (e.SubscriptionName == "MESSAGE1") SetLabelText(lblMessage1Subscribers, e.Count.ToString());
        //    else if (e.SubscriptionName == "MESSAGE2") SetLabelText(lblMessage2Subscribers, e.Count.ToString());
        //}

        //private void diagnostics_CounterTotalAbortedClientsChanged(object sender, SockMstrCounterChangedArgs e)
        //{
        //    SetLabelText(lblAbortedClients, e.Count.ToString());
        //}

        //void diagnostics_TotalConnectedClientsChanged(object sender, SockMstrCounterChangedArgs e)
        //{
        //    SetLabelText(lblTotalConnectedClients, e.Count.ToString());
        //}
        //void diagnostics_TotalMessagesReceivedChanged(object sender, SockMstrCounterChangedArgs e)
        //{
        //    SetLabelText(lblTotalMessagesReceived, e.Count.ToString());
        //}
        //void diagnostics_TotalMessagesSentChanged(object sender, SockMstrCounterChangedArgs e)
        //{
        //    SetLabelText(lblTotalMessagesSent, e.Count.ToString());
        //}
        //void diagnostics_CounterTotalBytesReceivedChanged(object sender, SockMstrCounterChangedArgs e)
        //{
        //    string val = "";
        //    val = e.Count.ToString("#,##0");
        //    SetLabelText(lblBytesReceived, val);
        //}
        //void diagnostics_CounterTotalBytesSentChanged(object sender, SockMstrCounterChangedArgs e)
        //{
        //    string val = "";
        //    val = e.Count.ToString("#,##0");
        //    SetLabelText(lblBytesSent, val);
        //}





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
        private void InsertListboxItem(string source, string text, int clientID, int endPointID, int messageID)
        {
            try
            {
                if (this.dGrid.InvokeRequired)
                {
                    ListBoxAddItemDelegate d = new ListBoxAddItemDelegate(InsertListboxItem);
                    this.Invoke(d, new object[] { source, text, clientID, endPointID, messageID });
                }
                else
                {
                    if (_gridItems.Count == 0) _gridItems.Add(new GridItem(source, text, clientID, endPointID, messageID));
                    else _gridItems.Insert(0, new GridItem(source, text, clientID, endPointID, messageID));

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
                //Global.Server.Start(tbIPAddress.Text, 100, cbCompressMessage.Checked, Convert.ToInt32(tbPort.Text), cbPolicyListener.Checked, cbEnableDiagnostics.Checked, cbEnableClientPinging.Checked);
                if (Global.Server != null)
                {
                    Global.Server.ExceptionRaised -= Server_ExceptionRaised;
                    Global.Server.MessageReceived -= socketServer_MessageReceived;
                    Global.Server.ListenerStateChanged -= Server_ListenerStateChanged;
                }

                Global.Server = new SocketServer(Convert.ToInt32(tbPort.Text), cbCompressMessage.Checked);
                Global.Server.ExceptionRaised += Server_ExceptionRaised;
                Global.Server.MessageReceived += socketServer_MessageReceived;
                Global.Server.ListenerStateChanged += Server_ListenerStateChanged;
                Global.Server.Start();

                btnStart.Enabled = false;       //  CHANGED WHEN EVENT IS RECEIVED
                pnlSettings.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                btnStart.Enabled = true;
            }
        }


        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                //  STOP
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
                SetButtonEnabled(btnSendM1Subs, true);
                SetButtonEnabled(btnSendM2Subs, true);
                SetLabelText(lblStatus, "STARTED");
            }
            else if (Status == SocketServerStatus.Stopped)
            {
                serviceStatusIndicator.BackColor = Color.Red;
                SetButtonEnabled(btnStart, true);
                SetButtonEnabled(btnStop, false);
                SetButtonEnabled(btnBroadcast, false);
                SetButtonEnabled(btnSendM1Subs, false);
                SetButtonEnabled(btnSendM2Subs, false);
                SetGroupBoxEnabled(pnlSettings, true);
                SetLabelText(lblStatus, "STOPPED");
            }
            else
            {
                serviceStatusIndicator.BackColor = Color.Orange;
                SetButtonEnabled(btnStart, false);
                SetButtonEnabled(btnStop, false);
                SetButtonEnabled(btnBroadcast, false);
                SetButtonEnabled(btnSendM1Subs, false);
                SetButtonEnabled(btnSendM2Subs, false);
            }

        }

        private void ExecuteBroadcaster()
        {
            while (true == true)
            {
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
        }



    }

}

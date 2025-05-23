﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketMeister
{
    public partial class UcSocketServer : UserControl
    {
        private readonly Random _rng = new Random();
        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private DateTime _nextAutomatedSend = DateTime.UtcNow;
        private string _messageText;
        private int _msProcessing = 2000;
        private int _msSendTimeout = 5000;
        private int _serverId;
        private int _port;
        private readonly object _lock = new object();

        private SocketServer _server = null;

        public event EventHandler<UiLogEventArgs> UiLogRaised;

        public UcSocketServer()
        {
            InitializeComponent();
        }

        public string MessageText
        {
            get { lock (_lock) { return _messageText; } }
            set { lock (_lock) { _messageText = value; } }
        }

        public DateTime NextAutomatedSend
        {
            get { lock (_lock) { return _nextAutomatedSend; } }
            set { lock (_lock) { _nextAutomatedSend = value; } }
        }

        public int Port
        {
            get => _port;
            set
            {
                _port = value;
                lbPort.Text = value.ToString();
            }
        }

        private SocketServer Server
        {
            get { lock (_lock) { return _server; } }
            set { lock (_lock) { _server = value; } }
        }

        public int ServerId
        {
            get => _serverId;
            set
            {
                _serverId = value;
                pnlMain.Text = "Socket Server #" + value.ToString();
            }
        }

        private void SetButtonEnabled(Button button, bool enabled)
        {
            if (button.InvokeRequired)
            {
                SetButtonEnabledDelegate d = new SetButtonEnabledDelegate(SetButtonEnabled);
                Invoke(d, new object[] { button, enabled });
            }
            else { button.Enabled = enabled; }
        }

        private void SetCheckBoxEnabled(CheckBox checkbox, bool enabled)
        {
            if (checkbox.InvokeRequired)
            {
                SetCheckBoxEnabledDelegate d = new SetCheckBoxEnabledDelegate(SetCheckBoxEnabled);
                Invoke(d, new object[] { checkbox, enabled });
            }
            else { checkbox.Enabled = enabled; }
        }

        private void SetLabelText(Label label, string text)
        {
            try
            {
                if (label.InvokeRequired)
                {
                    SetLabelTextDelegate d = new SetLabelTextDelegate(SetLabelText);
                    Invoke(d, new object[] { label, text });
                }
                else { label.Text = text; }
            }
            catch { }
        }

        public void Start()
        {
            try
            {
                Server = new SocketServer(_port, cbCompressMessage.Checked);
                Server.ClientConnected += Server_ClientConnected;
                Server.ClientDisconnected += Server_ClientDisconnected;
                Server.LogRaised += UcSocketServer_LogRaised;
                Server.MessageReceived += Server_MessageReceived;
                Server.StatusChanged += Server_StatusChanged;
                Server.Start();

                SetButtonEnabled(btnStart, false);       //  CHANGED WHEN EVENT IS RECEIVED
                SetCheckBoxEnabled(cbCompressMessage, false);
            }
            catch
            {
                SetButtonEnabled(btnStart, true);       //  CHANGED WHEN EVENT IS RECEIVED
                throw;
            }
        }

        public void Stop(bool AppExiting)
        {
            try
            {
                if (AppExiting == true)
                {
                    Server.ClientConnected -= Server_ClientConnected;
                    Server.ClientDisconnected -= Server_ClientDisconnected;
                    Server.LogRaised -= UcSocketServer_LogRaised;
                    Server.MessageReceived -= Server_MessageReceived;
                    Server.StatusChanged -= Server_StatusChanged;
                    Server.Stop();
                }
                else
                {
                    Server.Stop();
                    Server.ClientConnected -= Server_ClientConnected;
                    Server.ClientDisconnected -= Server_ClientDisconnected;
                    Server.LogRaised -= UcSocketServer_LogRaised;
                    Server.MessageReceived -= Server_MessageReceived;
                    Server.StatusChanged -= Server_StatusChanged;
                    SetButtonEnabled(btnStop, false);
                    SetButtonEnabled(btnSendMessage, false);
                    SetButtonEnabled(btnBroadcastToSubscribers, false);
                }
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }




        private void Server_ClientDisconnected(object sender, SocketServer.ClientEventArgs e)
        {
            SetLabelText(lblTotalConnectedClients, Server.ClientCount.ToString("N0"));
        }

        private void Server_ClientConnected(object sender, SocketServer.ClientEventArgs e)
        {
            SetLabelText(lblTotalConnectedClients, Server.ClientCount.ToString("N0"));
        }

        private void UcSocketServer_LogRaised(object sender, LogEventArgs e)
        {
            if (e.LogEntry.Severity == Severity.Error)
            {

                UiLogEventArgs args = new UiLogEventArgs(e.LogEntry.Exception, "Server", "Server");
                UiLogRaised?.Invoke(this, args);
            }
        }

        private void Server_StatusChanged(object sender, EventArgs e)
        {
            SetUI(Server.Status);
        }


        private void Server_MessageReceived(object sender, SocketServer.MessageReceivedEventArgs e)
        {
            SetLabelText(lblTotalRequestsReceived, Server.TotalMessagesReceived.ToString("N0"));
            SetLabelText(lblBytesReceived, Server.TotalBytesReceived.ToString("N0"));

            //  MESSAGE RECEIVED. SEND IT BACK
            int clientId = (int)e.Parameters[0];
            byte[] receivedBytes = (byte[])e.Parameters[1];
            string msgRec = Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);
            if (msgRec.Length > 60) msgRec = msgRec.Substring(0, 60);
            UiLogEventArgs args = new UiLogEventArgs(Severity.Information, "Server " + ServerId, "Client " + clientId, msgRec);
            UiLogRaised?.Invoke(this, args);

            //  Simulate Processing Time
            int msProcessing;
            lock (_lock) { msProcessing = _msProcessing; }            
            DateTime timeout = DateTime.Now.AddMilliseconds(msProcessing);
            while (DateTime.Now < timeout)
            {
                Thread.Sleep(1000);
            }

            //  Return message
            byte[] toSend = new byte[msgRec.Length];
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(msgRec), 0, toSend, 0, toSend.Length);
            e.Response = receivedBytes;
        }



        private void SetUI(SocketServerStatus Status)
        {
            if (Status == SocketServerStatus.Started)
            {
                serviceStatusIndicator.BackColor = Color.Green;
                SetButtonEnabled(btnStart, false);
                SetButtonEnabled(btnStop, true);
                SetButtonEnabled(btnSendMessage, true);
                SetButtonEnabled(btnBroadcastToSubscribers, true);
            }
            else if (Status == SocketServerStatus.Stopped)
            {
                serviceStatusIndicator.BackColor = Color.Red;
                SetButtonEnabled(btnStart, true);
                SetButtonEnabled(btnStop, false);
                SetButtonEnabled(btnSendMessage, false);
                SetButtonEnabled(btnBroadcastToSubscribers, false);
                SetCheckBoxEnabled(cbCompressMessage, true);
            }
            else
            {
                serviceStatusIndicator.BackColor = Color.Orange;
                SetButtonEnabled(btnStart, false);
                SetButtonEnabled(btnStop, false);
                SetButtonEnabled(btnSendMessage, false);
                SetButtonEnabled(btnBroadcastToSubscribers, false);
            }

        }

        public async void SendAutomatedMessage(UcSocketServer UcServer)
        {
            //  SEND A RANDOM MESSAGE OF A RANDOM SIZE (UP TO 1 MB)
            if (Server == null || Server.Status != SocketServerStatus.Started) return;

            if (rbAutoBCLarge.Checked == true)
            {
                string toSendStr = GenerateRandomString(_rng.Next(6048576) + 1000000);
                byte[] toSend = new byte[toSendStr.Length];
                Buffer.BlockCopy(Encoding.UTF8.GetBytes(toSendStr), 0, toSend, 0, toSend.Length);
                int itemsProcessed = await SendMessageToClientsAsync(toSend, false);
                if (itemsProcessed > 0) UcServer.NextAutomatedSend = DateTime.UtcNow.AddMilliseconds(500);
                else UcServer.NextAutomatedSend = DateTime.UtcNow.AddMilliseconds(1000);

            }
            else if (rbAutoBCFast.Checked == true)
            {
                int totalSent = 0;
                string toSendStr = GenerateRandomString(_rng.Next(10000));
                byte[] toSend = new byte[toSendStr.Length];
                Buffer.BlockCopy(Encoding.UTF8.GetBytes(toSendStr), 0, toSend, 0, toSend.Length);
                for (int i = 0; i < 10; i++)
                {
                    totalSent += await SendMessageToClientsAsync(toSend, false);
                    Thread.Sleep(50);
                }
            }
            else return;

        }


        private string GenerateRandomString(int size)
        {
            char[] buffer = new char[size];

            for (int i = 0; i < size; i++)
            {
                buffer[i] = _chars[_rng.Next(_chars.Length)];
            }
            return new string(buffer);
        }

        private async Task<int> SendMessageToClientsAsync(byte[] bytes, bool raiseLogOnResponse)
        {
            int msSendTimeout;
            lock (_lock) { msSendTimeout = _msSendTimeout; }

            List<SocketServer.Client> items = Server.GetClients();
            if (items.Count == 0) return 0;

            // Create a list to hold the tasks for parallel execution
            var tasks = new List<Task>();

            foreach (SocketServer.Client client in items)
            {
                object[] parms = { bytes };
                tasks.Add(Task.Run((Action)(() =>
                {
                    try
                    {
                        client.SendMessage(parms, msSendTimeout);
                    }
                    catch (Exception e)
                    {
                        UiLogEventArgs args = new UiLogEventArgs(e, "Server " + ServerId, "");
                        UiLogRaised?.Invoke(this, args);
                    }
                })));
            }

            // Wait for all the SendMessage tasks to complete
            await Task.WhenAll(tasks);

            // Update UI elements after all messages have been sent
            SetLabelText(lblTotalMessagesSent, Server.TotalMessagesSent.ToString("N0"));
            SetLabelText(lblBytesSent, Server.TotalBytesSent.ToString("N0"));

            if (raiseLogOnResponse)
            {
                UiLogEventArgs args = new UiLogEventArgs(Severity.Information, "Server " + ServerId, "", "Completed 'Send Message (Wait for Response)'");
                UiLogRaised?.Invoke(this, args);
            }
            return items.Count;
        }


        private void BtnBroadcastToSubscribers_Click(object sender, EventArgs e)
        {
            string msg = "Message to clients subscribing to \"My Test Subscription 1\"";
            byte[] toSend = new byte[msg.Length];
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(msg), 0, toSend, 0, toSend.Length);

            object[] parms = new object[1];
            parms[0] = toSend;

            _server.BroadcastToSubscribers("My Test Subscription 1", parms);

            SetLabelText(lblTotalMessagesSent, Server.TotalMessagesSent.ToString("N0"));
            SetLabelText(lblBytesSent, Server.TotalBytesSent.ToString("N0"));
        }

        private async void BtnSendMessage_Click(object sender, EventArgs e)
        {
            if (MessageText.Length == 0)
            {
                MessageBox.Show("Message Text must not be empty");
                return;
            }

            byte[] toSend = Encoding.UTF8.GetBytes(MessageText);

            try
            {
                int result = await SendMessageToClientsAsync(toSend, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while sending messages: {ex.Message}");
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            Start();
        }


        private void BtnStop_Click(object sender, EventArgs e)
        {
            Stop(false);
        }

        private void ShowException(Exception ex)
        {
            string e = ex.Message;
            if (ex.StackTrace != null)
            {
                e = e + Environment.NewLine + Environment.NewLine + ex.StackTrace;
            }
            MessageBox.Show(e, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void NmSndTimeout_ValueChanged(object sender, EventArgs e)
        {
            lock (_lock) { _msSendTimeout = (int)nmSndTimeout.Value; }
        }

        private void NmReceiveProcessing_ValueChanged(object sender, EventArgs e)
        {
            lock (_lock) {  _msProcessing = (int)nmReceiveProcessing.Value;}
        }
    }
}

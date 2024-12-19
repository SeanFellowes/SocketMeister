using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SocketMeister
{
    public partial class UcSocketServer : UserControl
    {
        private readonly Random _rng = new Random();
        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private DateTime _nextAutomatedSend = DateTime.Now;
        private string _messageText;
        private int _serverId;
        private int _port;
        private readonly object _lock = new object();

        private SocketServer _server = null;

        public event EventHandler<LogEventArgs> LogEventRaised;

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
                Server.TraceEventRaised += Server_TraceEventRaised;
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
                    Server.TraceEventRaised -= Server_TraceEventRaised;
                    Server.MessageReceived -= Server_MessageReceived;
                    Server.StatusChanged -= Server_StatusChanged;
                    Server.Stop();
                }
                else
                {
                    Server.Stop();
                    Server.ClientConnected -= Server_ClientConnected;
                    Server.ClientDisconnected -= Server_ClientDisconnected;
                    Server.TraceEventRaised -= Server_TraceEventRaised;
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

        private void Server_TraceEventRaised(object sender, TraceEventArgs e)
        {
            if (e.Severity == SeverityType.Error)
            {
                LogEventRaised?.Invoke(this, new LogEventArgs(new Exception(e.Message), "Server #" + ServerId.ToString(), "-"));
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

            //MESSAGE RECEIVED. SEND IT BACK IF LOGGING IS ON
            int clientId = (int)e.Parameters[0];
            byte[] receivedBytes = (byte[])e.Parameters[1];
            string msgRec = Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);
            if (msgRec.Length > 60)
                LogEventRaised?.Invoke(this, new LogEventArgs(SeverityType.Information, "Server #" + ServerId.ToString(), "Client " + clientId, "MessageReceived: " + msgRec.Substring(0, 60) + "..."));
            else
                LogEventRaised?.Invoke(this, new LogEventArgs(SeverityType.Information, "Server #" + ServerId.ToString(), "Client " + clientId, "MessageReceived: " + msgRec));


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

        public int SendAutomatedMessage()
        {
            //  SEND A RANDOM MESSAGE OF A RANDOM SIZE (UP TO 1 MB)
            if (Server == null || Server.Status != SocketServerStatus.Started) return 0;

            if (rbAutoBCLarge.Checked == true)
            {
                string toSendStr = GenerateRandomString(_rng.Next(6048576) + 1000000);
                byte[] toSend = new byte[toSendStr.Length];
                Buffer.BlockCopy(Encoding.UTF8.GetBytes(toSendStr), 0, toSend, 0, toSend.Length);
                return SendMessageToClients(toSend);
            }
            else if (rbAutoBCFast.Checked == true)
            {
                int totalSent = 0;
                string toSendStr = GenerateRandomString(_rng.Next(10000));
                byte[] toSend = new byte[toSendStr.Length];
                Buffer.BlockCopy(Encoding.UTF8.GetBytes(toSendStr), 0, toSend, 0, toSend.Length);
                for (int i = 0; i < 10; i++)
                {
                    totalSent += SendMessageToClients(toSend);
                    Thread.Sleep(50);
                }
                return totalSent;
            }
            else return 0;

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



        private int SendMessageToClients(byte[] Bytes)
        {
            List<SocketServer.Client> items = Server.GetClients();
            if (items.Count == 0) return 0;
            foreach (SocketServer.Client i in items)
            {
                object[] parms = new object[1];
                parms[0] = Bytes;
                try
                {
                    i.SendMessage(parms);
                }
                catch (Exception e)
                {
                    LogEventRaised?.Invoke(this, new LogEventArgs(SeverityType.Warning, "Server #" + ServerId.ToString(), "Client", "Error: " + e.ToString()));
                }
            }
            SetLabelText(lblTotalMessagesSent, Server.TotalMessagesSent.ToString("N0"));
            SetLabelText(lblBytesSent, Server.TotalBytesSent.ToString("N0"));
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

        private void BtnSendMessage_Click(object sender, EventArgs e)
        {
            if (MessageText.Length == 0) { MessageBox.Show("Message Text must not be empty"); return; }

            byte[] toSend = new byte[MessageText.Length];
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(MessageText), 0, toSend, 0, toSend.Length);
            SendMessageToClients(toSend);
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

    }
}

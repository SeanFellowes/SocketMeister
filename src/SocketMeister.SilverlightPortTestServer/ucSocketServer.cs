using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SocketMeister
{
    public partial class UcSocketServer : UserControl
    {
        private DateTime _nextAutomatedSend = DateTime.Now;
        private string _messageText = "I had planned on using drones early on—not too much because I think drones can be so overused. But I wanted to also shoot from the hornet's POV. Hornets articulate themselves in a totally different way than just the normal drone beauty shot. That's when I got tipped off about racing drones, which I had not used before. They're smaller, and the way they can articulate through the forest on a dime is very different from the regular drone.";
        private int _port = 4502;
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
            if (label.InvokeRequired)
            {
                SetLabelTextDelegate d = new SetLabelTextDelegate(SetLabelText);
                Invoke(d, new object[] { label, text });
            }
            else { label.Text = text; }
        }

        public void Start()
        {
            try
            {
                Server = new SocketServer(_port, false);
                Server.TraceEventRaised += Server_TraceEventRaised;
                Server.MessageReceived += Server_MessageReceived;
                Server.StatusChanged += Server_StatusChanged;
                Server.Start();

                SetButtonEnabled(btnStart, false);       //  CHANGED WHEN EVENT IS RECEIVED
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
                    Server.TraceEventRaised -= Server_TraceEventRaised;
                    Server.MessageReceived -= Server_MessageReceived;
                    Server.StatusChanged -= Server_StatusChanged;
                    Server.Stop();
                }
                else
                {
                    Server.Stop();
                    Server.TraceEventRaised -= Server_TraceEventRaised;
                    Server.MessageReceived -= Server_MessageReceived;
                    Server.StatusChanged -= Server_StatusChanged;
                    SetButtonEnabled(btnStop, false);
                    SetButtonEnabled(btnSendMessage, false);
                }
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }





        private void Server_TraceEventRaised(object sender, TraceEventArgs e)
        {
            if (e.Severity == SeverityType.Error)
            {
                LogEventRaised?.Invoke(this, new LogEventArgs(new Exception(e.Message), "Port #" + Port.ToString(), "-"));
            }
        }

        private void Server_StatusChanged(object sender, EventArgs e)
        {
            SetUI(Server.Status);
        }


        private void Server_MessageReceived(object sender, SocketServer.MessageReceivedEventArgs e)
        {
            //MESSAGE RECEIVED. SEND IT BACK IF LOGGING IS ON
            int clientId = (int)e.Parameters[0];
            byte[] receivedBytes = (byte[])e.Parameters[1];
            string msgRec = Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);

            LogEventRaised?.Invoke(this, new LogEventArgs(SeverityType.Information, "Port #" + Port.ToString(), "Client " + clientId, "MessageReceived: " + msgRec));

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
            }
            else if (Status == SocketServerStatus.Stopped)
            {
                serviceStatusIndicator.BackColor = Color.Red;
                SetButtonEnabled(btnStart, true);
                SetButtonEnabled(btnStop, false);
                SetButtonEnabled(btnSendMessage, false);
            }
            else
            {
                serviceStatusIndicator.BackColor = Color.Orange;
                SetButtonEnabled(btnStart, false);
                SetButtonEnabled(btnStop, false);
                SetButtonEnabled(btnSendMessage, false);
            }

        }


        private int SendMessageToClients(byte[] Bytes)
        {
            object[] parms = new object[1];
            parms[0] = Bytes;

            List<SocketServer.Client> items = Server.GetClients();
            if (items.Count == 0) return 0;
            foreach (SocketServer.Client i in items)
            {
                i.SendMessage(parms);
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

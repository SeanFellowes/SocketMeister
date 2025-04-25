using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using SocketMeister;

namespace SocketMeister.MiniTestClient
{
    internal class ResponseReceived : EventArgs
    {
        private readonly int _messageId;
        private readonly string _displayText;
        private readonly Severity severity;

        public ResponseReceived(Severity severity, int MessageId, string displayText)
        {
            this.severity = severity;
            _displayText = displayText;
            _messageId = MessageId;
        }

        public int MessageId => _messageId;
        public string DisplayText => _displayText;
        public Severity Severity => severity;
    }


    /// <summary>
    /// Interaction logic for ClientControl.xaml
    /// </summary>
    public partial class ClientControl : UserControl
    {
        private int _clientId = 1;
        private int _messagesReceived = 0;
        private int _messagesSent = 0;
        private readonly Random _rnd = new Random();
        private int _broadcastsReceived = 0;
        private bool _isRunning;
        private SocketClient _client;
        private readonly object _lock = new object();

        /// <summary>
        /// Event raised when an exception occurs
        /// </summary>
        internal event EventHandler<ResponseReceived> ResponseReceived;

        public event EventHandler<EventArgs> SendMessageButtonPressed;

        public event EventHandler<EventArgs> StatusChanged;

        /// <summary>
        /// Event raised whenever a broadcast is received from the server.
        /// </summary>
        public event EventHandler<SocketClient.BroadcastReceivedEventArgs> BroadcastReceived;

        /// <summary>
        /// Raised when a  message is received from the server. A response can be provided which will be returned to the server.
        /// </summary>
        public event EventHandler<SocketClient.MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Raised when the server service is stopping in a cleanly (Not a crash).
        /// </summary>
        public event EventHandler<EventArgs> ServerStopping;

        public event EventHandler<LogEventArgs> LogRaised;


        public ClientControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            tbPort.Text = "-";
        }

        public int ClientId
        {
            get => _clientId;
            set
            {
                _clientId = value;
                tbClientId.Text = value.ToString();
            }
        }


        public bool IsRunning
        {
            get { lock (_lock) { return _isRunning; } }
            set { lock (_lock) { _isRunning = value; } }
        }


        public bool TestSubscriptions
        {
            get => cbSubscriptions.IsChecked.Value;
            set => cbSubscriptions.IsChecked = value;
        }

        public bool TraceEvents
        {
            get => cbTrace.IsChecked.Value;
            set => cbTrace.IsChecked = value;
        }

        public SocketClient.ConnectionStatuses Status
        {
            get
            {
                if (_client == null) return SocketClient.ConnectionStatuses.Disconnected;
                else return _client.ConnectionStatus;
            }
        }

        public void ProcessSubscriptions()
        {
            if (cbSubscriptions.IsChecked == false) return;

            int cnt = _rnd.Next(1, 21);
            int direction = _rnd.Next(1, 3);


            if (direction == 1)
            {
                for (int ctr = 1; ctr <= cnt; ctr++)
                {
                    _client.AddSubscription(Guid.NewGuid().ToString());
                }
            }
            else
            {
                for (int ctr = 1; ctr <= cnt; ctr++)
                {
                    List<string> subs = _client.GetSubscriptions();
                    if (subs.Count > 0) _client.RemoveSubscription(subs[0]);
                    else break;
                }
            }

            if (_client.DoesSubscriptionNameExist("My Test Subscription 1") == false)
            {
                _client.AddSubscription("My Test Subscription 1");
            }

            int count = _client.GetSubscriptions().Count;
            tbSubscriptions.Text = count.ToString();
        }

        public void SendMessage(string Message, int TimeoutMs)
        {
            DateTime start = DateTime.Now;
            try
            {
                if (string.IsNullOrEmpty(Message)) throw new ArgumentNullException("Message cannot be null or empty");
                if (_client == null) throw new Exception("Client is null");

                new Thread(new ThreadStart(delegate
                {
                    try
                    {
                        byte[] toSend = new byte[Message.Length];
                        Buffer.BlockCopy(Encoding.UTF8.GetBytes(Message), 0, toSend, 0, toSend.Length);
                        object[] p = new object[2];
                        p[0] = _clientId;
                        p[1] = toSend;
                        byte[] result = _client.SendMessage(p, TimeoutMs);

                        string msg = "Response Received (" + (int)(DateTime.Now - start).TotalMilliseconds + " ms))";

                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            ResponseReceived?.Invoke(this, new ResponseReceived(Severity.Information, 0, msg));
                            _messagesSent++;
                            tbRequestsSent.Text = _messagesSent.ToString();
                        }));
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            LogRaised?.Invoke(this, new LogEventArgs(new LogEntry(ex)));
                        }));
                    }
                }))
                { IsBackground = true }.Start();
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    LogRaised?.Invoke(this, new LogEventArgs(new LogEntry(ex)));
                }));
            }
        }


        public void Start(List<SocketEndPoint> eps)
        {
            IsRunning = true;
            _client = new SocketClient(eps, true, "Client " + ClientId);
            _client.ConnectionStatusChanged += Client_ConnectionStatusChanged;
            _client.CurrentEndPointChanged += Client_CurrentEndPointChanged;
            _client.LogRaised += Client_LogRaised;
            _client.MessageReceived += Client_MessageReceived;
            _client.ServerStopping += Client_ServerStopping;
            _client.BroadcastReceived += Client_BroadcastReceived;
#if !VERSION4
            _client.LogRaised += Client_LogRaised;
#endif

            tbPort.Text = _client.CurrentEndPoint.Port.ToString();
        }


        public void Stop()
        {
            if (_client == null) throw new Exception("Client has not been started");

            _client.Stop();
            _client.Dispose();
        }

        private void Client_ConnectionStatusChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (_client.ConnectionStatus == SocketClient.ConnectionStatuses.Connected) bdStatus.Background = new SolidColorBrush(Colors.Green);
                else if (_client.ConnectionStatus == SocketClient.ConnectionStatuses.Disconnected) bdStatus.Background = new SolidColorBrush(Colors.Red);
                else bdStatus.Background = new SolidColorBrush(Colors.Orange);
                StatusChanged?.Invoke(this, new EventArgs());
            }));
        }

        private void Client_CurrentEndPointChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                tbPort.Text = _client.CurrentEndPoint.Port.ToString();
            }));
        }


        private void Client_BroadcastReceived(object sender, SocketClient.BroadcastReceivedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                _broadcastsReceived++;
                tbMessagesReceived.Text = _broadcastsReceived.ToString();
                BroadcastReceived?.Invoke(this, e);
            }));
        }


        private void Client_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            _messagesReceived++;
            Dispatcher.BeginInvoke((Action)(() =>
            {
                tbRequestsReceived.Text = _messagesReceived.ToString();
            }));
            MessageReceived?.Invoke(this, e);
        }

        private void Client_ServerStopping(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ServerStopping?.Invoke(this, e);
            }));
        }

        private void Client_LogRaised(object sender, LogEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (cbTrace.IsChecked == true) LogRaised?.Invoke(this, e);
            }));
        }



        private void BtnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            SendMessageButtonPressed?.Invoke(this, new EventArgs());
        }

    }

}

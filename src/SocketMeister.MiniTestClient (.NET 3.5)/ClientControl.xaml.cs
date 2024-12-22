using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SocketMeister.MiniTestClient
{
    public partial class ClientControl : UserControl
    {
        private int _clientId = 1;
        private int _messagesReceived = 0;
        private int _messagesSent = 0;
        private readonly Random _rnd = new Random();
        private int _broadcastsReceived = 0;
        private SocketClient _client;

        public event EventHandler<ExceptionEventArgs> ExceptionRaised;
        public event EventHandler<EventArgs> SendMessageButtonPressed;
        public event EventHandler<EventArgs> StatusChanged;
        public event EventHandler<SocketClient.BroadcastReceivedEventArgs> BroadcastReceived;
        public event EventHandler<SocketClient.MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<EventArgs> ServerStopping;

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

        public bool TestSubscriptions
        {
            get => cbSubscriptions.IsChecked.Value;
            set => cbSubscriptions.IsChecked = value;
        }

        public SocketClient.ConnectionStatuses Status
        {
            get => _client == null ? SocketClient.ConnectionStatuses.Disconnected : _client.ConnectionStatus;
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

        public void SendMessage(string message)
        {
            try
            {
                if (string.IsNullOrEmpty(message)) throw new ArgumentNullException("Message cannot be null or empty");
                if (_client == null) throw new Exception("Client is null");

                byte[] toSend = Encoding.UTF8.GetBytes(message);
                object[] p = { _clientId, toSend };
                byte[] result = _client.SendMessage(p);

                Dispatcher.BeginInvoke((Action)(() =>
                {
                    _messagesSent++;
                    tbRequestsSent.Text = _messagesSent.ToString();
                }));
            }
            catch (Exception ex)
            {
                ExceptionRaised?.Invoke(this, new ExceptionEventArgs(ex, 1200));
            }
        }

        public void Start(string ipAddress, int port)
        {
            _client = new SocketClient(ipAddress, port, true);
            _client.ConnectionStatusChanged += Client_ConnectionStatusChanged;
            _client.CurrentEndPointChanged += Client_CurrentEndPointChanged;
            _client.ExceptionRaised += Client_ExceptionRaised;
            _client.MessageReceived += Client_MessageReceived;
            _client.ServerStopping += Client_ServerStopping;
            _client.BroadcastReceived += Client_BroadcastReceived;

            tbPort.Text = _client.CurrentEndPoint.Port.ToString();
        }

        public void Start(List<SocketEndPoint> eps)
        {
            _client = new SocketClient(eps, true);
            _client.ConnectionStatusChanged += Client_ConnectionStatusChanged;
            _client.CurrentEndPointChanged += Client_CurrentEndPointChanged;
            _client.ExceptionRaised += Client_ExceptionRaised;
            _client.MessageReceived += Client_MessageReceived;
            _client.ServerStopping += Client_ServerStopping;
            _client.BroadcastReceived += Client_BroadcastReceived;

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
                if (_client.ConnectionStatus == SocketClient.ConnectionStatuses.Connected)
                {
                    bdStatus.Background = new SolidColorBrush(Colors.Green);
                }
                else if (_client.ConnectionStatus == SocketClient.ConnectionStatuses.Disconnected)
                {
                    bdStatus.Background = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    bdStatus.Background = new SolidColorBrush(Colors.Orange);
                }
            }));
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Client_CurrentEndPointChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                tbPort.Text = _client.CurrentEndPoint.Port.ToString();
            }));
        }

        private void Client_ExceptionRaised(object sender, ExceptionEventArgs e)
        {
            ExceptionRaised?.Invoke(this, e);
        }

        private void Client_BroadcastReceived(object sender, SocketClient.BroadcastReceivedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                _broadcastsReceived++;
                tbMessagesReceived.Text = _broadcastsReceived.ToString();
            }));
            BroadcastReceived?.Invoke(this, e);
        }

        private void Client_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                _messagesReceived++;
                tbRequestsReceived.Text = _messagesReceived.ToString();
            }));
            MessageReceived?.Invoke(this, e);
        }

        private void Client_ServerStopping(object sender, EventArgs e)
        {
            ServerStopping?.Invoke(this, e);
        }

        private void BtnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            SendMessageButtonPressed?.Invoke(this, EventArgs.Empty);
        }
    }
}

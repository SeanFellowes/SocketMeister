using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SocketMeister.MiniTestClient
{
    /// <summary>
    /// Interaction logic for ClientControl.xaml
    /// </summary>
    public partial class ClientControl : UserControl
    {
        private int _clientId = 1;
        private int _messagesReceived = 0;
        private int _messagesSent = 0;
        private readonly Random _rnd = new();
        private int _broadcastsReceived = 0;
        private SocketClient _client;

        /// <summary>
        /// Event raised when an exception occurs
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ExceptionRaised;

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

        public void SendMessage(string Message)
        {
            try
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                if (string.IsNullOrEmpty(Message)) throw new ArgumentNullException("Message cannot be null or empty");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
                if (_client == null) throw new Exception("Client is null");

                byte[] toSend = new byte[Message.Length];
                Buffer.BlockCopy(Encoding.UTF8.GetBytes(Message), 0, toSend, 0, toSend.Length);
                object[] p = new object[2];
                p[0] = _clientId;
                p[1] = toSend;
                byte[] result = _client.SendMessage(p);

                Dispatcher.Invoke(() =>
                {
                    _messagesSent++;
                    tbRequestsSent.Text = _messagesSent.ToString();

                });

            }
            catch (Exception ex)
            {
                ExceptionRaised?.Invoke(this, new ExceptionEventArgs(ex, 1200));
            }
        }

        public void Start(string IPAddress, int Port)
        {
            _client = new SocketClient(IPAddress, Port, true);
            _client.ConnectionStatusChanged += Client_ConnectionStatusChanged;
            _client.CurrentEndPointChanged += Client_CurrentEndPointChanged;
            _client.ExceptionRaised += Client_ExceptionRaised;
            //_client.MessageReceived += Client_MessageReceived;
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
            //_client.MessageReceived += Client_MessageReceived;
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
            Dispatcher.Invoke(() =>
            {
                if (_client.ConnectionStatus == SocketClient.ConnectionStatuses.Connected) bdStatus.Background = new SolidColorBrush(Colors.Green);
                else if (_client.ConnectionStatus == SocketClient.ConnectionStatuses.Disconnected) bdStatus.Background = new SolidColorBrush(Colors.Red);
                else bdStatus.Background = new SolidColorBrush(Colors.Orange);
            });
            StatusChanged?.Invoke(this, new EventArgs());
        }

        private void Client_CurrentEndPointChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                tbPort.Text = _client.CurrentEndPoint.Port.ToString();
            });
        }



        private void Client_ExceptionRaised(object sender, ExceptionEventArgs e)
        {
            ExceptionRaised?.Invoke(this, e);
        }

        private void Client_BroadcastReceived(object sender, SocketClient.BroadcastReceivedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _broadcastsReceived++;
                tbMessagesReceived.Text = _broadcastsReceived.ToString();
            });
            BroadcastReceived?.Invoke(this, e);
        }


        private void Client_MessageReceived(object sender, SocketClient.MessageReceivedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _messagesReceived++;
                tbRequestsReceived.Text = _messagesReceived.ToString();
            });
            MessageReceived?.Invoke(this, e);
        }

        private void Client_ServerStopping(object sender, EventArgs e)
        {
            ServerStopping?.Invoke(this, e);
        }

        private void BtnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            SendMessageButtonPressed?.Invoke(this, new EventArgs());
        }
    }

}
